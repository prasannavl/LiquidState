using System.Diagnostics.Contracts;
using LiquidState.Common;
using LiquidState.Configuration;
using LiquidState.Core;
using LiquidState.Synchronous.Core;

namespace LiquidState.Synchronous
{
    public abstract class RawStateMachineBase<TState, TTrigger> : AbstractStateMachine<TState, TTrigger>
    {
        protected RawStateMachineBase(TState initialState, Configuration<TState, TTrigger> configuration)
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

        public override void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            if (!IsEnabled) return;
            ExecutionHelper.MoveToStateCore(state, option, this);
        }

        public override void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (!IsEnabled) return;
            ExecutionHelper.FireCore(parameterizedTrigger, argument, this);
        }

        public override void Fire(TTrigger trigger)
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