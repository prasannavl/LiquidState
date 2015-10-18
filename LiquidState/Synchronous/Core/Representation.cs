// Author: Prasanna V. Loganathar
// Created: 09:55 16-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using LiquidState.Core;

namespace LiquidState.Synchronous.Core
{
    internal class StateRepresentation<TState, TTrigger>
    {
        public readonly TState State;
        public readonly List<TriggerRepresentation<TTrigger>> Triggers;
        public Action<Transition<TState, TTrigger>> OnEntryAction;
        public Action<Transition<TState, TTrigger>> OnExitAction;

        internal StateRepresentation(TState state)
        {
            State = state;
            // Allocate with capacity as 1 to avoid wastage of memory.
            Triggers = new List<TriggerRepresentation<TTrigger>>(1);
        }
    }

    internal class TriggerRepresentation<TTrigger>
    {
        public readonly TTrigger Trigger;
        public Func<bool> ConditionalTriggerPredicate;
        public object NextStateRepresentationWrapper;
        public object OnTriggerAction;
        public TransitionFlag TransitionFlags;

        internal TriggerRepresentation(TTrigger trigger)
        {
            Trigger = trigger;
        }
    }

    [Flags]
    internal enum TransitionFlag
    {
        None = 0,
        DynamicState = 1,
    }
}