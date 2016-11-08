// Author: Prasanna V. Loganathar
// Created: 04:13 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using LiquidState.Common;
using LiquidState.Core;
using LiquidState.Synchronous.Core;

namespace LiquidState.Synchronous
{
    public abstract class GuardedStateMachineBase<TState, TTrigger> : RawStateMachineBase<TState, TTrigger>
    {
        private InterlockedMonitor m_monitor = new InterlockedMonitor();

        protected GuardedStateMachineBase(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration) {}

        public override void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            if (m_monitor.TryEnter())
            {
                try { base.MoveToState(state, option); }
                finally { m_monitor.Exit(); }
            }
            else
            {
                if (IsEnabled) ExecutionHelper.ThrowInTransition();
            }
        }

        public override void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (m_monitor.TryEnter())
            {
                try { base.Fire(parameterizedTrigger, argument); }
                finally { m_monitor.Exit(); }
            }
            else
            {
                if (IsEnabled) ExecutionHelper.ThrowInTransition();
            }
        }

        public override void Fire(TTrigger trigger)
        {
            if (m_monitor.TryEnter())
            {
                try { base.Fire(trigger); }
                finally { m_monitor.Exit(); }
            }
            else
            {
                if (IsEnabled) ExecutionHelper.ThrowInTransition();
            }
        }
    }

    public sealed class GuardedStateMachine<TState, TTrigger> : GuardedStateMachineBase<TState, TTrigger>
    {
        public GuardedStateMachine(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration) {}
    }
}