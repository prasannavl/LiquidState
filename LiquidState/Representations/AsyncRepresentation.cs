using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Representations
{
    internal class AsyncStateRepresentation<TState, TTrigger>
    {
        internal AsyncStateRepresentation(TState state)
        {
            Contract.Requires(state != null);

            Contract.Ensures(State != null);
            Contract.Ensures(Triggers != null);

            State = state;
            // Allocate with capacity as 1 to avoid wastage of memory.
            Triggers = new List<AsyncTriggerRepresentation<TTrigger, TState>>(1);
        }

        public object OnEntryAction;
        public object OnExitAction;
        public readonly TState State;
        public AsyncStateTransitionFlag TransitionFlags;
        public readonly List<AsyncTriggerRepresentation<TTrigger, TState>> Triggers;
    }

    internal class AsyncTriggerRepresentation<TTrigger, TState>
    {
        internal AsyncTriggerRepresentation(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Ensures(Trigger != null);

            Trigger = trigger;
        }

        public object ConditionalTriggerPredicate;
        public object OnTriggerAction;
        public object WrappedTriggerAction;
        public AsyncStateRepresentation<TState, TTrigger> NextStateRepresentation;
        public AsyncStateTransitionFlag TransitionFlags;
        public readonly TTrigger Trigger;
    }

    [Flags]
    internal enum AsyncStateTransitionFlag : byte
    {
        None = 0x0,
        Synchronous = 0x01,
        EntryReturnsTask = 0x02,
        ExitReturnsTask = 0x04,
        TriggerActionReturnsTask = 0x08,
        TriggerPredicateReturnsTask = 0x10,
    }
}
