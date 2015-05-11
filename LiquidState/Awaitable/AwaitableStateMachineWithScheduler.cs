// Author: Prasanna V. Loganathar
// Created: 1:30 AM 05-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LiquidState.Awaitable.Core;
using LiquidState.Core;

namespace LiquidState.Awaitable
{
    public class AwaitableStateMachineWithScheduler<TState, TTrigger> : IAwaitableStateMachine<TState, TTrigger>
    {
        private readonly AwaitableStateMachine<TState, TTrigger> machine;

        internal AwaitableStateMachineWithScheduler(TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> config, TaskScheduler scheduler)
        {
            Contract.Requires(initialState != null);
            Contract.Requires(config != null);
            Contract.Requires(scheduler != null);

            machine = new AwaitableStateMachine<TState, TTrigger>(initialState, config);
            Scheduler = scheduler;
        }

        public TaskScheduler Scheduler { get; private set; }

        public event Action<TTrigger, TState> UnhandledTriggerExecuted
        {
            add { machine.UnhandledTriggerExecuted += value; }
            remove { machine.UnhandledTriggerExecuted -= value; }
        }

        public event Action<TState, TState> StateChanged
        {
            add { machine.StateChanged += value; }
            remove { machine.StateChanged -= value; }
        }

        public Task<bool> CanHandleTriggerAsync(TTrigger trigger)
        {
            return machine.CanHandleTriggerAsync(trigger);
        }

        public bool CanTransitionTo(TState state)
        {
            return machine.CanTransitionTo(state);
        }

        public Task MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            return !IsEnabled
                ? Task.FromResult(false)
                : RunOnScheduler(() => machine.MoveToStateInternal(state, option));
        }

        public void Pause()
        {
            machine.Pause();
        }

        public void Resume()
        {
            machine.Resume();
        }

        public Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            return !IsEnabled
                ? Task.FromResult(false)
                : RunOnScheduler(() => machine.FireInternalAsync(parameterizedTrigger, argument));
        }

        public Task FireAsync(TTrigger trigger)
        {
            return !IsEnabled ? Task.FromResult(false) : RunOnScheduler(() => machine.FireInternalAsync(trigger));
        }

        public bool IsInTransition
        {
            get { return machine.IsInTransition; }
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

        private Task RunOnScheduler(Func<Task> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None,
                TaskCreationOptions.None, Scheduler).Unwrap();
        }
    }
}
