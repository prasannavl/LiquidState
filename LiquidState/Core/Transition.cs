using System;

namespace LiquidState.Core
{
    public struct Transition<TState, TTrigger>
    {
        private readonly TState source;
        private readonly TState destination;
        private readonly TTrigger trigger;
        private readonly bool hasTrigger;

        public Transition(TState source, TState destination)
        {
            this.source = source;
            this.destination = destination;
            trigger = default(TTrigger);
            hasTrigger = false;
        }

        public Transition(TState source, TState destination, TTrigger trigger)
        {
            this.source = source;
            this.destination = destination;
            this.trigger = trigger;
            hasTrigger = true;
        }

        public TState Source
        {
            get { return source; }
        }

        public TState Destination
        {
            get { return destination; }
        }

        public TTrigger Trigger
        {
            get { return trigger; }
        }

        public bool HasTrigger
        {
            get { return hasTrigger; }
        }
    }

    public class TriggerStateEventArgs<TState, TTrigger> : EventArgs
    {
        public TriggerStateEventArgs(TState currentState, TTrigger trigger)
        {
            Trigger = trigger;
            CurrentState = currentState;
        }

        public TTrigger Trigger { get; private set; }
        public TState CurrentState { get; private set; }
    }

    public class OptionalTriggerStateEventArgs<TState, TTrigger> : TriggerStateEventArgs<TState, TTrigger>
    {
        protected OptionalTriggerStateEventArgs(TState currentState, TTrigger trigger, bool hasTrigger = true)
            : base(currentState, trigger)
        {
            HasTrigger = hasTrigger;
        }

        protected OptionalTriggerStateEventArgs(TState currentState) : base(currentState, default(TTrigger))
        {
            HasTrigger = false;
        }

        public bool HasTrigger { get; private set; }
    }


    public class TransitionEventArgs<TState, TTrigger> : OptionalTriggerStateEventArgs<TState, TTrigger>
    {
        public TransitionEventArgs(TState currentState, TState targetState, TTrigger trigger, bool hasTrigger = true)
            : base(currentState, trigger, hasTrigger)
        {
            TargetState = targetState;
        }

        public TransitionEventArgs(TState currentState, TState targetState) : base(currentState)
        {
            TargetState = targetState;
        }

        public TState TargetState { get; private set; }
    }

    public class TransitionExecutedEventArgs<TState, TTrigger> : OptionalTriggerStateEventArgs<TState, TTrigger>
    {
        public TransitionExecutedEventArgs(TState currentState, TState pastState, TTrigger trigger,
            bool hasTrigger = true)
            : base(currentState, trigger, hasTrigger)
        {
            PastState = pastState;
        }

        public TransitionExecutedEventArgs(TState currentState, TState pastState)
            : base(currentState)
        {
            PastState = pastState;
        }

        public TState PastState { get; private set; }
    }
}
