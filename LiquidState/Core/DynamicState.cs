// Author: Prasanna V. Loganathar
// Created: 13:32 17-06-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

namespace LiquidState
{
    public static class DynamicState
    {
        public static DynamicState<TState> Create<TState>(TState state, bool canTransition = true)
        {
            return new DynamicState<TState>(state, canTransition);
        }

        public static DynamicState<TState> NoTransition<TState>()
        {
            return DynamicState<TState>.NoTransition;
        }
    }

    public struct DynamicState<TState>
    {
        public static DynamicState<TState> NoTransition = new DynamicState<TState>(default(TState), false);
        public TState ResultingState;
        public bool CanTransition;

        public DynamicState(TState state, bool canTransition = true)
        {
            ResultingState = state;
            CanTransition = canTransition;
        }
    }
}