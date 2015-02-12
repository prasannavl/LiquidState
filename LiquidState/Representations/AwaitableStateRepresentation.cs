// Author: Prasanna V. Loganathar
// Created: 3:31 PM 07-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Representations
{
    internal class AwaitableStateRepresentation<TState, TTrigger>
    {
        public readonly TState State;
        public readonly List<AwaitableTriggerRepresentation<TTrigger, TState>> Triggers;
        public object OnEntryAction;
        public object OnExitAction;
        public AwaitableStateTransitionFlag TransitionFlags;

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
        public readonly TTrigger Trigger;
        public object ConditionalTriggerPredicate;
        public AwaitableStateRepresentation<TState, TTrigger> NextStateRepresentation;
        public object OnTriggerAction;
        public AwaitableStateTransitionFlag TransitionFlags;

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
        OverrideFluidState = 0x01,
        FluidStateActive = 0x02,
        EntryReturnsTask = 0x04,
        ExitReturnsTask = 0x08,
        TriggerActionReturnsTask = 0x10,
        TriggerPredicateReturnsTask = 0x20
    }
}