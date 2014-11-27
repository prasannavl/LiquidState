// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Representations
{
    internal class AsyncStateRepresentation<TState, TTrigger>
    {
        public object OnEntryAction;
        public object OnExitAction;
        public AsyncStateTransitionFlag TransitionFlags;
        public readonly TState State;
        public readonly List<AsyncTriggerRepresentation<TTrigger, TState>> Triggers;

        internal AsyncStateRepresentation(TState state)
        {
            Contract.Requires(state != null);

            Contract.Ensures(State != null);
            Contract.Ensures(Triggers != null);

            State = state;
            // Allocate with capacity as 1 to avoid wastage of memory.
            Triggers = new List<AsyncTriggerRepresentation<TTrigger, TState>>(1);
        }
    }

    internal class AsyncTriggerRepresentation<TTrigger, TState>
    {
        public object ConditionalTriggerPredicate;
        public AsyncStateRepresentation<TState, TTrigger> NextStateRepresentation;
        public object OnTriggerAction;
        public AsyncStateTransitionFlag TransitionFlags;
        public object WrappedTriggerAction;
        public readonly TTrigger Trigger;

        internal AsyncTriggerRepresentation(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Ensures(Trigger != null);

            Trigger = trigger;
        }
    }

    [Flags]
    internal enum AsyncStateTransitionFlag : byte
    {
        None = 0x0,
        Synchronous = 0x01,
        EntryReturnsTask = 0x02,
        ExitReturnsTask = 0x04,
        TriggerActionReturnsTask = 0x08,
        TriggerPredicateReturnsTask = 0x10
    }
}