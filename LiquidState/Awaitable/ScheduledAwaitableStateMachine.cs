// Author: Prasanna V. Loganathar
// Created: 04:13 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using LiquidState.Awaitable.Core;
using LiquidState.Core;

namespace LiquidState.Awaitable
{
    public abstract class ScheduledAwaitableStateMachineBase<TState, TTrigger> :
        RawAwaitableStateMachineBase<TState, TTrigger>
    {
        protected ScheduledAwaitableStateMachineBase(TState initialState,
            AwaitableConfiguration<TState, TTrigger> awaitableConfiguration, TaskScheduler scheduler)
            : base(initialState, awaitableConfiguration)
        {
            Scheduler = scheduler;
        }

        public TaskScheduler Scheduler { get; }

        public override Task MoveToStateAsync(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            return RunOnScheduler(() => base.MoveToStateAsync(state, option));
        }

        public override Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            return RunOnScheduler(() => base.FireAsync(parameterizedTrigger, argument));
        }

        public override Task FireAsync(TTrigger trigger)
        {
            return RunOnScheduler(() => base.FireAsync(trigger));
        }

        public virtual Task RunOnScheduler(Func<Task> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None,
                TaskCreationOptions.None, Scheduler).Unwrap();
        }
    }

    public sealed class ScheduledAwaitableStateMachine<TState, TTrigger> :
        ScheduledAwaitableStateMachineBase<TState, TTrigger>
    {
        public ScheduledAwaitableStateMachine(TState initialState,
            AwaitableConfiguration<TState, TTrigger> awaitableConfiguration,
            TaskScheduler scheduler)
            : base(initialState, awaitableConfiguration, scheduler)
        {
        }
    }
}