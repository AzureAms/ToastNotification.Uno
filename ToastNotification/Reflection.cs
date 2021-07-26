using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Uno.Extras
{
    internal static class Reflection
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static TResult Invoke<TObject, TResult>(this TObject obj, string name, object[] args)
        {
            var methodInfo = typeof(TObject).GetMethod(name, flags);
            return (TResult)methodInfo.Invoke(obj, args);
        }

        public static void Invoke<TObject>(this TObject obj, string name, object[] args)
        {
            var methodInfo = typeof(TObject).GetMethod(name, flags);
            methodInfo.Invoke(obj, args);
        }

        /// <summary>
        /// Wraps around the lengthy syntax of C# to get MethodInfo
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="obj">Owner object</param>
        /// <param name="name">Name of method</param>
        /// <returns></returns>
        public static MethodInfo GetMethod<TObject>(this TObject obj, string name)
        {
            return typeof(TObject).GetMethod(name, flags);
        }

        public static TObject Construct<TObject>(params object[] args)
        {
            var constructor = typeof(TObject).GetConstructor(
                flags ^ BindingFlags.Static,
                null,
                args.Select(arg => arg.GetType()).ToArray(),
                null);

            return (TObject)constructor.Invoke(args);
        }

        public static TValue GetValue<TObject, TValue>(this TObject obj, string name)
        {
            var fieldInfo = typeof(TObject).GetField(name, flags);
            if (fieldInfo != null)
            {
                return (TValue)fieldInfo.GetValue(obj);
            }
            var propertyInfo = typeof(TObject).GetProperty(name, flags);
            return (TValue)propertyInfo.GetValue(obj);
        }
    }
}
