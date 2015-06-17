// Author: Prasanna V. Loganathar
// Created: 3:31 PM 07-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Awaitable.Core
{
    internal class StateRepresentation<TState, TTrigger>
    {
        public readonly TState State;
        public readonly List<TriggerRepresentation<TTrigger, TState>> Triggers;
        public object OnEntryAction;
        public object OnExitAction;
        public TransitionFlag TransitionFlags;

        internal StateRepresentation(TState state)
        {
            Contract.Requires(state != null);

            Contract.Ensures(State != null);
            Contract.Ensures(Triggers != null);

            State = state;
            // Allocate with capacity as 1 to avoid wastage of memory.
            Triggers = new List<TriggerRepresentation<TTrigger, TState>>(1);
        }
    }

    internal class TriggerRepresentation<TTrigger, TState>
    {
        public readonly TTrigger Trigger;
        public object ConditionalTriggerPredicate;
        public object NextStateRepresentationWrapper;
        public object OnTriggerAction;
        public TransitionFlag TransitionFlags;

        internal TriggerRepresentation(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Ensures(Trigger != null);

            Trigger = trigger;
        }
    }

    [Flags]
    internal enum TransitionFlag
    {
        None = 0,
        EntryReturnsTask = 1,
        ExitReturnsTask = 1 << 1,
        TriggerActionReturnsTask = 1 << 2,
        TriggerPredicateReturnsTask = 1 << 3,
        DynamicState = 1 << 4,
        DynamicStateReturnsTask = DynamicState | 1 << 5,
    }
}
