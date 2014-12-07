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
    public class QueuedAwaitableStateMachine<TState, TTrigger> : IAwaitableStateMachine<TState, TTrigger>
    {
        private static Task<bool> cachedFalseTask = Task.FromResult(false);

        private IImmutableQueue<Action> actionsQueue;
        private SynchronizationContext context;
        private volatile bool isPaused;
        private AwaitableStateMachine<TState, TTrigger> machine;

        public QueuedAwaitableStateMachine(TState initialState, AwaitableStateMachineConfiguration<TState, TTrigger> config,
            SynchronizationContext context = null)
        {
            machine = new AwaitableStateMachine<TState, TTrigger>(initialState, config);
            machine.UnhandledTriggerExecuted += UnhandledTriggerExecuted;
            machine.StateChanged += StateChanged;
            this.context = context ?? SynchronizationContext.Current ?? new SynchronizationContext();
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
                    Action action = () => context.Post(async o =>
                    {
                        await machine.FireAsync(parameterizedTrigger, argument);
                        tcs.SetResult(true);
                    }, null);

                    Interlocked.CompareExchange(ref actionsQueue, actionsQueue.Enqueue(action), actionsQueue);
                }
                else
                {
                    context.Post(async o =>
                    {
                        await machine.FireAsync(parameterizedTrigger, argument);
                        tcs.SetResult(true);
                    }, null);
                }

                return tcs.Task;
            }
            return cachedFalseTask;
        }

        public Task FireAsync(TTrigger trigger)
        {
            if (IsEnabled)
            {
                var tcs = new TaskCompletionSource<bool>();

                if (isPaused)
                {
                    Action action = () => context.Post(async o =>
                    {
                        await machine.Fire(trigger);
                        tcs.SetResult(true);
                    }, null);

                    Interlocked.CompareExchange(ref actionsQueue, actionsQueue.Enqueue(action), actionsQueue);
                }
                else
                {
                    context.Post(async o =>
                    {
                        await machine.Fire(trigger);
                        tcs.SetResult(true);
                    }, null);
                }

                return tcs.Task;
            }
            return cachedFalseTask;
        }
    }
}