using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Uno.Extras;

namespace Notification.Natives
{
    // Yes, we could have included the code from the GtkSharp profject,
    // but that code is LGPL so we want to stay safe.
    internal static class GtkFuncLoader
    {
        private static Type _loaderType;
        private static Type _gLibraryType;
        private static Type _libraryType;

        static GtkFuncLoader()
        {
            var glibAssembly = typeof(GLib.Application).Assembly; 
            _loaderType = glibAssembly.GetType("FuncLoader");
            _gLibraryType = glibAssembly.GetType("GLibrary");
            _libraryType = glibAssembly.GetType("Library");
        }

        public static IntPtr Load(string library)
        {
            var libraryEnum = Enum.Parse(_libraryType, library);
            return _gLibraryType.InvokeStatic<IntPtr>("Load", new [] { libraryEnum });
        }

        public static TDelegate LoadFunction<TDelegate>(string library, string functionName) 
            where TDelegate : Delegate
        {
            var functionPtr = _loaderType.InvokeStatic<IntPtr>("GetProcAddress", new object[] { Load(library), functionName });
            
            if (functionPtr != IntPtr.Zero)
            {
                return Marshal.GetDelegateForFunctionPointer<TDelegate>(functionPtr);
            }
            
            return default(TDelegate);
        }
    }
}