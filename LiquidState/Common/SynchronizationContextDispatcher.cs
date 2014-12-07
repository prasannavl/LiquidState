// Author: Prasanna V. Loganathar
// Created: 11:24 PM 10-11-2014

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LiquidState.Common
{
    public class SynchronizationContextDispatcher : IDispatcher
    {
        private static SynchronizationContext _uiContext;
        private static int _managedThreadId;


        public void Initialize()
        {
            _managedThreadId = Environment.CurrentManagedThreadId;
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAccess()
        {
            return _managedThreadId == Environment.CurrentManagedThreadId;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(Action action)
        {
            if (CheckAccess())
            {
                action();
            }
            else
            {
                _uiContext.Post(o => action(), null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute<T>(Action<T> action, T state)
        {
            if (CheckAccess())
            {
                action(state);
            }
            else
            {
                _uiContext.Post(s => action((T)s), state);                
            }
        }
    }
}