// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics.Contracts;
using LiquidState.Core;
using LiquidState.Synchronous.Core;

namespace LiquidState.Synchronous
{
    public abstract class BlockingStateMachineBase<TState, TTrigger> : RawStateMachineBase<TState, TTrigger>
    {
        private readonly object syncRoot = new object();

        protected BlockingStateMachineBase(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);
        }

        public override void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            lock (syncRoot)
            {
                base.MoveToState(state, option);
            }
        }

        public override void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            lock (syncRoot)
            {
                base.Fire(parameterizedTrigger, argument);
            }
        }

        public override void Fire(TTrigger trigger)
        {
            lock (syncRoot)
            {
                base.Fire(trigger);
            }
        }
    }

    public sealed class BlockingStateMachine<TState, TTrigger> : BlockingStateMachineBase<TState, TTrigger>
    {
        public BlockingStateMachine(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);
        }
    }
}
