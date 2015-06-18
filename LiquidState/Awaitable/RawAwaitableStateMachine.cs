// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LiquidState.Awaitable.Core;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Awaitable
{
    public abstract class RawAwaitableStateMachineBase<TState, TTrigger> : AbstractStateMachineCore<TState, TTrigger>,
        IAwaitableStateMachine<TState, TTrigger>
    {
        internal AwaitableStateRepresentation<TState, TTrigger> CurrentAwaitableStateRepresentation;
        internal Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> Representations;

        protected RawAwaitableStateMachineBase(TState initialState,
            AwaitableConfiguration<TState, TTrigger> awaitableConfiguration)
        {
            Contract.Requires(awaitableConfiguration != null);
            Contract.Requires(initialState != null);

            CurrentAwaitableStateRepresentation = awaitableConfiguration.GetInitialStateRepresentation(initialState);
            if (CurrentAwaitableStateRepresentation == null)
            {
                InvalidStateException<TState>.Throw(initialState);
            }

            Representations = awaitableConfiguration.Representations;
        }

        public virtual Task MoveToStateAsync(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            return !IsEnabled ? TaskHelpers.CompletedTask : AwaitableExecutionHelper.MoveToStateCoreAsync(state, option, this);
        }

        public Task<bool> CanHandleTriggerAsync(TTrigger trigger, bool exactMatch = false)
        {
            return AwaitableExecutionHelper.CanHandleTriggerAsync(trigger, this, exactMatch);
        }

        public Task<bool> CanHandleTriggerAsync(TTrigger trigger, Type argumentType)
        {
            return AwaitableExecutionHelper.CanHandleTriggerAsync(trigger, this, argumentType);
        }

        public Task<bool> CanHandleTriggerAsync<TArgument>(TTrigger trigger)
        {
            return AwaitableExecutionHelper.CanHandleTriggerAsync<TState, TTrigger, TArgument>(trigger, this);
        }

        public virtual Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            return !IsEnabled ? TaskHelpers.CompletedTask : AwaitableExecutionHelper.FireCoreAsync(parameterizedTrigger, argument, this);
        }

        public virtual Task FireAsync(TTrigger trigger)
        {
            return !IsEnabled ? TaskHelpers.CompletedTask : AwaitableExecutionHelper.FireCoreAsync(trigger, this);
        }

        public override IEnumerable<TTrigger> CurrentPermittedTriggers
        {
            get
            {
                foreach (var triggerRepresentation in CurrentAwaitableStateRepresentation.Triggers)
                {
                    yield return triggerRepresentation.Trigger;
                }
            }
        }

        public override TState CurrentState
        {
            get { return CurrentAwaitableStateRepresentation.State; }
        }
    }

    public sealed class RawAwaitableStateMachine<TState, TTrigger> : RawAwaitableStateMachineBase<TState, TTrigger>
    {
        public RawAwaitableStateMachine(TState initialState, AwaitableConfiguration<TState, TTrigger> awaitableConfiguration)
            : base(initialState, awaitableConfiguration)
        {
            Contract.Requires(awaitableConfiguration != null);
            Contract.Requires(initialState != null);
        }
    }
}
