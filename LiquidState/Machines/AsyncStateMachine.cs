using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiquidState.Common;
using LiquidState.Configuration;

namespace LiquidState.Machines
{
    public class AsyncStateMachine<TState, TTrigger> : IAwaitableStateMachine<TState, TTrigger>
    {
        private IImmutableQueue<Action> actionsQueue;
        private volatile bool isPaused;
        private AwaitableStateMachine<TState, TTrigger> machine;
        private IDispatcher dispatcher;

        public AsyncStateMachine(TState initialState, AwaitableStateMachineConfiguration<TState, TTrigger> config)
        {
            machine = new AwaitableStateMachine<TState, TTrigger>(initialState, config);
            machine.UnhandledTriggerExecuted += UnhandledTriggerExecuted;
            machine.StateChanged += StateChanged;
            dispatcher = new SynchronizationContextDispatcher();
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
            get { return machine.IsEnabled; }
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
            actionsQueue = ImmutableQueue.Create<Action>();
        }

        public void Resume()
        {
            isPaused = false;
            foreach (var action in actionsQueue)
            {
                action();
            }
            actionsQueue = null;
        }

        public Task Stop()
        {
            return machine.Stop();
        }

        public event Action<TTrigger, TState> UnhandledTriggerExecuted;
        public event Action<TState, TState> StateChanged;

        public Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument)
        {
            if (IsEnabled)
            {
                var tcs = new TaskCompletionSource<bool>();

                if (isPaused)
                {
                    Action action = () => dispatcher.Execute(async () =>
                    {
                        await machine.FireAsync(parameterizedTrigger, argument);
                        tcs.SetResult(true);
                    });

                    Interlocked.CompareExchange(ref actionsQueue, actionsQueue.Enqueue(action), actionsQueue);
                }
                else
                {
                    dispatcher.Execute(async () =>
                    {
                        await machine.FireAsync(parameterizedTrigger, argument);
                        tcs.SetResult(true);
                    });
                }

                return tcs.Task;
            }
            return TaskCache.FalseTask;
        }

        public Task FireAsync(TTrigger trigger)
        {
            if (IsEnabled)
            {
                var tcs = new TaskCompletionSource<bool>();

                if (isPaused)
                {
                    Action action = () => dispatcher.Execute(async () =>
                    {
                        await machine.FireAsync(trigger);
                        tcs.SetResult(true);
                    });

                    Interlocked.CompareExchange(ref actionsQueue, actionsQueue.Enqueue(action), actionsQueue);
                }
                else
                {
                    dispatcher.Execute(async () =>
                    {
                        await machine.FireAsync(trigger);
                        tcs.SetResult(true);
                    });
                }

                return tcs.Task;
            }
            return TaskCache.FalseTask;
        }
    }
}