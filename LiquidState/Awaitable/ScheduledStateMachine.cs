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
    public abstract class ScheduledStateMachineBase<TState, TTrigger> : RawStateMachineBase<TState, TTrigger>
    {
        protected ScheduledStateMachineBase(TState initialState,
            Configuration<TState, TTrigger> configuration, TaskScheduler scheduler)
            : base(initialState, configuration)
        {
            Contract.Requires(initialState != null);
            Contract.Requires(configuration != null);
            Contract.Requires(scheduler != null);

            Scheduler = scheduler;
        }

        public TaskScheduler Scheduler { get; private set; }

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

    public sealed class ScheduledStateMachine<TState, TTrigger> : ScheduledStateMachineBase<TState, TTrigger>
    {
        public ScheduledStateMachine(TState initialState, Configuration<TState, TTrigger> configuration,
            TaskScheduler scheduler)
            : base(initialState, configuration, scheduler)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);
        }
    }
}
