using System;

namespace LiquidState.Core
{
    public class TriggerStateEventArgs<TState, TTrigger> : EventArgs
    {
        public TTrigger Trigger { get; set; }
        public TState CurrentState { get; set; }

        public TriggerStateEventArgs(TState currentState, TTrigger trigger)
        {
            Trigger = trigger;
            CurrentState = currentState;
        }
    }

    public class OptionalTriggerStateEventArgs<TState, TTrigger> : TriggerStateEventArgs<TState, TTrigger>
    {
        public bool HasTrigger { get; private set; }

        protected OptionalTriggerStateEventArgs(TState currentState, TTrigger trigger, bool hasTrigger = true) : base(currentState, trigger)
        {
            HasTrigger = hasTrigger;
        }

        protected OptionalTriggerStateEventArgs(TState currentState) : base(currentState, default(TTrigger))
        {
            HasTrigger = false;
        }
    }


    public class TransitionEventArgs<TState, TTrigger> : OptionalTriggerStateEventArgs<TState, TTrigger>
    {
        public TState TargetState { get; set; }

        public TransitionEventArgs(TState currentState, TState targetState, TTrigger trigger, bool hasTrigger = true) : base(currentState, trigger, hasTrigger)
        {
            TargetState = targetState;
        }

        public TransitionEventArgs(TState currentState, TState targetState) : base(currentState)
        {
            TargetState = targetState;
        }
    }

    public class TransitionExecutedEventArgs<TState, TTrigger> : OptionalTriggerStateEventArgs<TState, TTrigger>
    {
        public TState PastState { get; set; }

        public TransitionExecutedEventArgs(TState currentState, TState pastState, TTrigger trigger, bool hasTrigger = true)
            : base(currentState, trigger, hasTrigger)
        {
            PastState = pastState;
        }

        public TransitionExecutedEventArgs(TState currentState, TState pastState)
            : base(currentState)
        {
            PastState = pastState;
        }
    }
}
