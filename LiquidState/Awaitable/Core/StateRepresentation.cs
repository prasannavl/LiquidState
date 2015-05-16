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
        public AwaitableStateTransitionFlag TransitionFlags;

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
        public object NextStateRepresentationPredicate;
        public object OnTriggerAction;
        public AwaitableStateTransitionFlag TransitionFlags;

        internal TriggerRepresentation(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Ensures(Trigger != null);

            Trigger = trigger;
        }
    }

    [Flags]
    internal enum AwaitableStateTransitionFlag
    {
        None = 0x0,
        EntryReturnsTask = 0x01,
        ExitReturnsTask = 0x02,
        TriggerActionReturnsTask = 0x04,
        TriggerPredicateReturnsTask = 0x08,
        NextStateRepresentationIsTargetStateTask = 0x10,
    }
}
