using System.Collections.Generic;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Synchronous.Core
{
    public abstract class AbstractStateMachine<TState, TTrigger> : AbstractStateMachineCore<TState, TTrigger>, IStateMachine<TState, TTrigger>
    {
        protected internal StateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        protected internal Dictionary<TState, StateRepresentation<TState, TTrigger>> Representations;

        public abstract void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument);

        public abstract void Fire(TTrigger trigger);
        public abstract void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default);

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
    }
}