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
    public abstract class RawStateMachineBase<TState, TTrigger> : AbstractStateMachineCore<TState, TTrigger>,
        IStateMachine<TState, TTrigger>
    {
        internal StateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        internal Dictionary<TState, StateRepresentation<TState, TTrigger>> Representations;

        protected RawStateMachineBase(TState initialState,
            Configuration<TState, TTrigger> configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);

            CurrentStateRepresentation = configuration.GetInitialStateRepresentation(initialState);
            if (CurrentStateRepresentation == null)
            {
                InvalidStateException<TState>.Throw(initialState);
            }

            Representations = configuration.Representations;
        }

        public virtual Task MoveToStateAsync(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            return !IsEnabled ? TaskHelpers.CompletedTask : ExecutionHelper.MoveToStateCoreAsync(state, option, this);
        }

        public virtual Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            return !IsEnabled ? TaskHelpers.CompletedTask : ExecutionHelper.FireCoreAsync(parameterizedTrigger, argument, this);
        }

        public virtual Task FireAsync(TTrigger trigger)
        {
            return !IsEnabled ? TaskHelpers.CompletedTask : ExecutionHelper.FireCoreAsync(trigger, this);
        }

        public override IEnumerable<TTrigger> CurrentPermittedTriggers
        {
            get
            {
                foreach (var triggerRepresentation in CurrentStateRepresentation.Triggers)
                {
                    yield return triggerRepresentation.Trigger;
                }
            }
        }

        public override TState CurrentState
        {
            get { return CurrentStateRepresentation.State; }
        }
    }

    public sealed class RawStateMachine<TState, TTrigger> : RawStateMachineBase<TState, TTrigger>
    {
        public RawStateMachine(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);
        }
    }
}
