// Author: Prasanna V. Loganathar
// Created: 04:18 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace LiquidState.Core
{
    public struct Transition<TState, TTrigger>
    {
        public Transition(TState source, TState destination)
        {
            this.Source = source;
            this.Destination = destination;
            Trigger = default(TTrigger);
            HasTrigger = false;
        }

        public Transition(TState source, TState destination, TTrigger trigger)
        {
            this.Source = source;
            this.Destination = destination;
            this.Trigger = trigger;
            HasTrigger = true;
        }

        public TState Source { get; }
        public TState Destination { get; }
        public TTrigger Trigger { get; }
        public bool HasTrigger { get; }
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