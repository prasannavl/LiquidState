// Author: Prasanna V. Loganathar
// Created: 11:24 PM 10-11-2014

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LiquidState.Common
{
    public class SynchronizationContextDispatcher : IDispatcher
    {
        private static SynchronizationContext _uiContext;

        public TaskScheduler Scheduler { get; private set; }

        public void Initialize()
        {
            _uiContext = SynchronizationContext.Current;
            if (_uiContext == null)
            {
                _uiContext = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(_uiContext);
            }
            Scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAccess()
        {
            return SynchronizationContext.Current == _uiContext;
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

    public static class DispatcherExtensions
    {
        public static Task ExecuteAsync(this IDispatcher dispatcher, Func<Task> task)
        {
            return Task.Factory.StartNew(task,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                dispatcher.Scheduler);
        }
    }
}