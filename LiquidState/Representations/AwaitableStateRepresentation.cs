using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Representations
{
    internal class AwaitableStateRepresentation<TState, TTrigger>
    {
        public object OnEntryAction;
        public object OnExitAction;
        public AwaitableStateTransitionFlag TransitionFlags;
        public readonly TState State;
        public readonly List<AwaitableTriggerRepresentation<TTrigger, TState>> Triggers;

        internal AwaitableStateRepresentation(TState state)
        {
            Contract.Requires(state != null);

            Contract.Ensures(State != null);
            Contract.Ensures(Triggers != null);

            State = state;
            // Allocate with capacity as 1 to avoid wastage of memory.
            Triggers = new List<AwaitableTriggerRepresentation<TTrigger, TState>>(1);
        }
    }

    internal class AwaitableTriggerRepresentation<TTrigger, TState>
    {
        public object ConditionalTriggerPredicate;
        public AwaitableStateRepresentation<TState, TTrigger> NextStateRepresentation;
        public object OnTriggerAction;
        public AwaitableStateTransitionFlag TransitionFlags;
        public readonly TTrigger Trigger;

        internal AwaitableTriggerRepresentation(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Ensures(Trigger != null);

            Trigger = trigger;
        }
    }

    [Flags]
    internal enum AwaitableStateTransitionFlag : byte
    {
        None = 0x0,
        Synchronous = 0x01,
        EntryReturnsTask = 0x02,
        ExitReturnsTask = 0x04,
        TriggerActionReturnsTask = 0x08,
        TriggerPredicateReturnsTask = 0x10
    }
}