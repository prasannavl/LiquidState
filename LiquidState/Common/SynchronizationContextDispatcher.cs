// Author: Prasanna V. Loganathar
// Created: 3:41 PM 07-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiquidState.Common
{
    public class SynchronizationContextDispatcher : IDispatcher
    {
        private static SynchronizationContext _uiContext;

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

        public bool CheckAccess()
        {
            return SynchronizationContext.Current == _uiContext;
        }

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

        public void Execute<T>(Action<T> action, T state)
        {
            if (CheckAccess())
            {
                action(state);
            }
            else
            {
                _uiContext.Post(s => action((T) s), state);
            }
        }

        public TaskScheduler Scheduler { get; private set; }
    }

    public static class DispatcherExtensions
    {
        public static Task ExecuteAsync(this IDispatcher dispatcher, Func<Task> task)
        {
            return Task.Factory.StartNew(task,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                dispatcher.Scheduler).Unwrap();
        }
    }
}