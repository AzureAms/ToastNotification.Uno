using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Notification.FrameworkDependent
{
    /// <summary>
    /// Compiles all CSharp and XAML files embedded in this assembly.
    /// </summary>
    internal static class Compiler
    {
        /// <summary>
        /// Compiles the dynamic control, and references Framework-specific assemblies.
        /// This method is slow, takes about 2 seconds on all targets.
        /// Therefore, it is recommended that the assembly should be compiled as soon as possible
        /// on a background thread.
        /// </summary>
        /// <returns>The Assembly that contains the dynamic control</returns>
        public static Assembly Compile()
        {
            return CompileInternal(Assembly.GetEntryAssembly());
        }

        // Used for Unit tests.
        internal static Assembly CompileInternal(Assembly entry, bool? shouldDebug = null)
        {
            bool isDebug = shouldDebug ?? Debugger.IsAttached;

            var syntaxTrees = EnumerateResourceNames("cs").Select(name => CSharpSyntaxTree.ParseText(ReadResource(name)));

            var compilation = CSharpCompilation.Create(
                "ToastNotification.Runtime.Wpf",
                syntaxTrees,
                GetReferences(entry).ToArray(),
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: isDebug ? OptimizationLevel.Debug : OptimizationLevel.Release)
                );

            var resourceDescriptions = EnumerateResourceNames("xaml")
                .Select(name =>
                    new ResourceDescription(
                        $"Notification.FrameworkDependent.{ExtractBareFileName(name)}",
                        () => SafeOpenResource(name),
                        true)
                );

            var assemblyStream = new MemoryStream();

            Assembly dynamicallyCompiledAssembly;

            if (isDebug)
            {
                var pdbStream = new MemoryStream();
                var emitOptions = new EmitOptions(
                    debugInformationFormat: DebugInformationFormat.PortablePdb);
                var embeddedTexts = EnumerateResourceNames("cs").Select(name =>
                {
                    return EmbeddedText.FromSource(ExtractBareFileName(name), SourceText.From(ReadResource(name), Encoding.UTF8));
                });
                var result = compilation.Emit(assemblyStream, pdbStream,
                    manifestResources: resourceDescriptions,
                    embeddedTexts: embeddedTexts,
                    options: emitOptions);

                // If it was not successful, throw an exception to fail the test
                if (!result.Success)
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        stringBuilder.AppendLine(diagnostic.ToString());
                    }

                    throw new InvalidOperationException(stringBuilder.ToString());
                }

                dynamicallyCompiledAssembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());
                pdbStream.Dispose();
            }
            else
            {
                _ = compilation.Emit(assemblyStream, manifestResources: resourceDescriptions);
                dynamicallyCompiledAssembly = Assembly.Load(assemblyStream.ToArray());
            }

            assemblyStream.Dispose();

            return dynamicallyCompiledAssembly;
        }

        private static string ReadResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        private static Stream OpenResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Opens a stream, copies it into a MemoryStream, and closes it.
        /// MemoryStreams do not control unmanaged resources, so failing
        /// to call Dispose() does not result in a leak.
        /// </summary>
        /// <param name="resourceName">Name of resource to open</param>
        /// <returns></returns>
        private static MemoryStream SafeOpenResource(string resourceName)
        {
            var stream = OpenResource(resourceName);
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            stream.Dispose();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private static IEnumerable<MetadataReference> GetReferences(Assembly entry)
        {
            // Tested on .NET 4.8, .NET Core 3.1, and the latest .NET 5.0

            var applicationType = entry.GetTypes().FirstOrDefault(t =>
            {
                // Clients might be using libraries that extends
                // System.Windows.Application
                // so we must check the whole class tree.
                do
                {
                    if (t.FullName == "System.Windows.Application")
                    {
                        return true;
                    }
                    t = t.BaseType;
                }
                while (t != null);

                return false;
            });

            if (applicationType == null)
            {
                throw new InvalidOperationException("This program is not a valid WPF application. Check your app's App.xaml");
            }

            var presentationFrameworkAssembly = applicationType.Assembly;

            // This is safe, `Assembly`s have their `GetHashCode` method overridden.
            var result = new HashSet<Assembly>();

            // Running a BFS, a classic thing for CPers.
            var q = new Queue<Assembly>();
            q.Enqueue(presentationFrameworkAssembly);

            while (q.Count > 0)
            {
                var asm = q.Dequeue();
                foreach (var childName in asm.GetReferencedAssemblies())
                {
                    try
                    {
                        var childAsm = Assembly.Load(childName);
                        if (result.Contains(childAsm))
                        {
                            continue;
                        }

                        q.Enqueue(childAsm);
                        result.Add(childAsm);
                    }
                    catch (FileNotFoundException)
                    {
                        Debug.WriteLine($"Cannot find {childName}");
                    }
                }
            }

            return result.Select(asm => { try { return asm.ToMetadataReference(); } catch { return null; } })
                         .Where(meta => meta != null);
        }

        private static MetadataReference ToMetadataReference(this Assembly asm)
        {
            return MetadataReference.CreateFromFile(asm.Location);
        }
        
        private static IEnumerable<string> EnumerateResourceNames(string extension)
        {
            var asm = Assembly.GetExecutingAssembly();

            return asm.GetManifestResourceNames()
                .Where(name => name.EndsWith($".{extension}", StringComparison.InvariantCultureIgnoreCase));
        }

        private static string ExtractBareFileName(string fileName)
        {
            var name = fileName.Split('.');
            return name[name.Length - 2] + "." + name[name.Length - 1];
        }
    }
}
