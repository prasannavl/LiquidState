// Author: Prasanna V. Loganathar
// Created: 09:55 16-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;

namespace LiquidState.Awaitable.Core
{
    internal class AwaitableStateRepresentation<TState, TTrigger>
    {
        public readonly TState State;
        public readonly List<AwaitableTriggerRepresentation<TTrigger>> Triggers;
        public AwaitableTransitionFlag AwaitableTransitionFlags;
        public object OnEntryAction;
        public object OnExitAction;

        internal AwaitableStateRepresentation(TState state)
        {
            State = state;
            // Allocate with capacity as 1 to avoid wastage of memory.
            Triggers = new List<AwaitableTriggerRepresentation<TTrigger>>(1);
        }
    }

    internal class AwaitableTriggerRepresentation<TTrigger>
    {
        public readonly TTrigger Trigger;
        public AwaitableTransitionFlag AwaitableTransitionFlags;
        public object ConditionalTriggerPredicate;
        public object NextStateRepresentationWrapper;
        public object OnTriggerAction;

        internal AwaitableTriggerRepresentation(TTrigger trigger)
        {
            Trigger = trigger;
        }
    }

    [Flags]
    internal enum AwaitableTransitionFlag
    {
        None = 0,
        EntryReturnsTask = 1,
        ExitReturnsTask = 1 << 1,
        TriggerActionReturnsTask = 1 << 2,
        TriggerPredicateReturnsTask = 1 << 3,
        DynamicState = 1 << 4,
        DynamicStateReturnsTask = DynamicState | (1 << 5)
    }
}