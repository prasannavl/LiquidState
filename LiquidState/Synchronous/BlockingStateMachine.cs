// Author: Prasanna V. Loganathar
// Created: 04:13 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using LiquidState.Core;
using LiquidState.Synchronous.Core;

namespace LiquidState.Synchronous
{
    public abstract class BlockingStateMachineBase<TState, TTrigger> : RawStateMachineBase<TState, TTrigger>
    {
        private readonly object m_syncObject = new object();

        protected BlockingStateMachineBase(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration) {}

        public override void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            lock (m_syncObject) { base.MoveToState(state, option); }
        }

        public override void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            lock (m_syncObject) { base.Fire(parameterizedTrigger, argument); }
        }

        public override void Fire(TTrigger trigger)
        {
            lock (m_syncObject) { base.Fire(trigger); }
        }
    }

    public sealed class BlockingStateMachine<TState, TTrigger> : BlockingStateMachineBase<TState, TTrigger>
    {
        public BlockingStateMachine(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration) {}
    }
}