namespace LiquidState.Core
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
