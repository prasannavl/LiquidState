// Author: Prasanna V. Loganathar
// Created: 5:11 PM 22-02-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;

namespace LiquidState.Common
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class ValidatedNotNullAttribute : Attribute { }

    internal static class Requires
    {
        [DebuggerStepThrough]
        public static void NotNull<T>([ValidatedNotNull] T value, string parameterName) where T : class
        {
            if (value != null)
                return;
            FailArgumentNullException(parameterName);
        }

        [DebuggerStepThrough]
        public static T NotNullPassthrough<T>([ValidatedNotNull] T value, string parameterName) where T : class
        {
            NotNull(value, parameterName);
            return value;
        }

        [DebuggerStepThrough]
        public static void NotNullAllowStructs<T>([ValidatedNotNull] T value, string parameterName)
        {
            if (value != null)
                return;
            FailArgumentNullException(parameterName);
        }

        [DebuggerStepThrough]
        public static void Range(bool condition, string parameterName, string message = null)
        {
            if (condition)
                return;
            FailRange(parameterName, message);
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

        [DebuggerStepThrough]
        private static void FailArgumentNullException(string parameterName)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}
