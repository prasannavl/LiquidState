using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LiquidState.Common;
using LiquidState.Core;
using LiquidState.Synchronous.Core;

namespace LiquidState.Synchronous
{
    public abstract class RawStateMachineBase<TState, TTrigger> : AbstractStateMachineCore<TState, TTrigger>, IStateMachine<TState, TTrigger>
    {
        internal StateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        internal Dictionary<TState, StateRepresentation<TState, TTrigger>> Representations;

        public override TState CurrentState
        {
            get { return CurrentStateRepresentation.State; }
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

        protected RawStateMachineBase(TState initialState, Configuration<TState, TTrigger> configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);

            Representations = configuration.Representations;

            CurrentStateRepresentation = StateConfigurationHelper<TState, TTrigger>.FindStateRepresentation(
                initialState, Representations);

            if (CurrentStateRepresentation == null)
            {
                InvalidStateException<TState>.Throw(initialState);
            }
        }

        public virtual void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            if (!IsEnabled) return;
            ExecutionHelper.MoveToStateCore(state, option, this);
        }

        public bool CanHandleTrigger(TTrigger trigger, bool exactMatch = false)
        {
            return ExecutionHelper.CanHandleTrigger(trigger, this, exactMatch);
        }

        public bool CanHandleTrigger(TTrigger trigger, Type argumentType)
        {
            return ExecutionHelper.CanHandleTrigger(trigger, this, argumentType);
        }

        public bool CanHandleTrigger<TArgument>(TTrigger trigger)
        {
            return ExecutionHelper.CanHandleTrigger<TState, TTrigger, TArgument>(trigger, this);
        }

        public virtual void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (!IsEnabled) return;
            ExecutionHelper.FireCore(parameterizedTrigger, argument, this);
        }

        public virtual void Fire(TTrigger trigger)
        {
            if (!IsEnabled) return;
            ExecutionHelper.FireCore(trigger, this);
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