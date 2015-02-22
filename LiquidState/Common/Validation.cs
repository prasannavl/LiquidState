using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidState.Common
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class ValidatedNotNullAttribute : Attribute
    {
    }
    internal static class Requires
    {
        [DebuggerStepThrough]
        public static void NotNull<T>([ValidatedNotNull] T value, string parameterName) where T : class
        {
            if ((object)value != null)
                return;
            Requires.FailArgumentNullException(parameterName);
        }

        [DebuggerStepThrough]
        public static T NotNullPassthrough<T>([ValidatedNotNull] T value, string parameterName) where T : class
        {
            Requires.NotNull<T>(value, parameterName);
            return value;
        }

        [DebuggerStepThrough]
        public static void NotNullAllowStructs<T>([ValidatedNotNull] T value, string parameterName)
        {
            if ((object)value != null)
                return;
            Requires.FailArgumentNullException(parameterName);
        }

        [DebuggerStepThrough]
        private static void FailArgumentNullException(string parameterName)
        {
            throw new ArgumentNullException(parameterName);
        }

        [DebuggerStepThrough]
        public static void Range(bool condition, string parameterName, string message = null)
        {
            if (condition)
                return;
            Requires.FailRange(parameterName, message);
        }

        [DebuggerStepThrough]
        public static void FailRange(string parameterName, string message = null)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentOutOfRangeException(parameterName);
            throw new ArgumentOutOfRangeException(parameterName, message);
        }

        [DebuggerStepThrough]
        public static void Argument(bool condition, string parameterName, string message)
        {
            if (!condition)
                throw new ArgumentException(message, parameterName);
        }

        [DebuggerStepThrough]
        public static void Argument(bool condition)
        {
            if (!condition)
                throw new ArgumentException();
        }

        [DebuggerStepThrough]
        public static void FailObjectDisposed<TDisposed>(TDisposed disposed)
        {
            throw new ObjectDisposedException(disposed.GetType().FullName);
        }
    }
}
