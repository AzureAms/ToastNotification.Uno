using NUnit.Framework;
using Notification.FrameworkDependent;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ToastNotification.Wpf.RuntimeCompilationTests
{
    [Apartment(System.Threading.ApartmentState.STA)]
    public class Given_Compiler : System.Windows.Application
    {
        [Test]
        public void When_CompileAndInitializeToast()
        {
            Debug.WriteLine("Compiling...");
            var asm = Compiler.CompileInternal(Assembly.GetExecutingAssembly(), true);

            Debug.WriteLine("Loading...");
            var loaderType = asm.GetTypes().FirstOrDefault(type => type.Name == "ToastNotificationLoader");

            dynamic loader = Activator.CreateInstance(loaderType);
        }
    }
}