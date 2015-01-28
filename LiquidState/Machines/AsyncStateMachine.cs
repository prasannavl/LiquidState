using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiquidState.Common;
using LiquidState.Configuration;

namespace LiquidState.Machines
{
    public class AsyncStateMachine<TState, TTrigger> : IAwaitableStateMachine<TState, TTrigger>
    {
        private IImmutableQueue<Func<Task>> actionsQueue;
        private readonly IDispatcher dispatcher;
        private volatile bool isInQueue;
        private volatile bool isPaused;
        private volatile bool isRunning;
        private AwaitableStateMachine<TState, TTrigger> machine;
        private volatile int queueCount;

        internal AsyncStateMachine(TState initialState, AwaitableStateMachineConfiguration<TState, TTrigger> config)
        {
            Contract.Requires(initialState != null);
            Contract.Requires(config != null);

            machine = new AwaitableStateMachine<TState, TTrigger>(initialState, config);
            machine.UnhandledTriggerExecuted += UnhandledTriggerExecuted;
            machine.StateChanged += StateChanged;
            dispatcher = new SynchronizationContextDispatcher();
            dispatcher.Initialize();
            actionsQueue = ImmutableQueue.Create<Func<Task>>();
        }

        public bool IsInTransition
        {
            get { return isRunning; }
        }

        public TState CurrentState
        {
            get { return machine.CurrentState; }
        }

        public IEnumerable<TTrigger> CurrentPermittedTriggers
        {
            get { return machine.CurrentPermittedTriggers; }
        }

        public bool IsEnabled
        {
            get { return !isPaused && machine.IsEnabled; }
        }

        public bool CanHandleTrigger(TTrigger trigger)
        {
            return machine.CanHandleTrigger(trigger);
        }

        public bool CanTransitionTo(TState state)
        {
            return machine.CanTransitionTo(state);
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Resume()
        {
            isPaused = false;
            var _ = RunFromQueueIfNotEmpty();
        }

        public Task Stop()
        {
            return machine.Stop();
        }

        public event Action<TTrigger, TState> UnhandledTriggerExecuted;
        public event Action<TState, TState> StateChanged;

        public Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (IsEnabled)
            {
                var tcs = new TaskCompletionSource<bool>();
                Func<Task> action = () => dispatcher.ExecuteAsync(async () =>
                {
                    try
                    {
                        await machine.FireAsync(parameterizedTrigger, argument);
                    }
                    finally
                    {
                        tcs.SetResult(true);

                        if (!isInQueue)
                        {
                            var _ = RunFromQueueIfNotEmpty();
                        }
                    }
                });

                if (isRunning || isPaused)
                {
                    lock (actionsQueue)
                    {
                        actionsQueue = actionsQueue.Enqueue(action);
                        queueCount++;
                    }
                    return tcs.Task;
                }

                isRunning = true;
                action();
                return tcs.Task;
            }
            return TaskCache.Completed;
        }

        public Task FireAsync(TTrigger trigger)
        {
            if (IsEnabled)
            {
                var tcs = new TaskCompletionSource<bool>();
                Func<Task> action = () => dispatcher.ExecuteAsync(async () =>
                {
                    try
                    {
                        await machine.FireAsync(trigger);
                    }
                    finally
                    {
                        tcs.SetResult(true);

                        if (!isInQueue)
                        {
                            var _ = RunFromQueueIfNotEmpty();                            
                        }
                    }
                });

                if (isRunning || isPaused)
                {
                    lock (actionsQueue)
                    {
                        actionsQueue = actionsQueue.Enqueue(action);
                        queueCount++;
                    }
                    return tcs.Task;
                }

                isRunning = true;
                action();
                return tcs.Task;
            }
            return TaskCache.Completed;
        }

        private async Task RunFromQueueIfNotEmpty()
        {
            isRunning = true;
            isInQueue = true;

            try
            {
                while (queueCount > 0)
                {
                    Func<Task> current = null;
                    lock (actionsQueue)
                    {
                        if (queueCount > 0)
                        {
                            current = actionsQueue.Peek();
                            actionsQueue = actionsQueue.Dequeue();
                            queueCount--;
                        }
                    }
                    if (current != null)
                        await current();
                }
            }
            finally
            {
                isInQueue = false;
                isRunning = false;
            }
        }
    }
}