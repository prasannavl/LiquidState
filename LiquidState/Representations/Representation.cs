// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Representations
{
    internal class StateRepresentation<TState, TTrigger>
    {
        public Action OnEntryAction;
        public Action OnExitAction;
        public readonly TState State;
        public readonly List<TriggerRepresentation<TTrigger, TState>> Triggers;

        internal StateRepresentation(TState state)
        {
            Contract.Requires(state != null);
            Contract.Ensures(State != null);

            State = state;
            // Allocate with capacity as 1 to avoid wastage of memory.
            Triggers = new List<TriggerRepresentation<TTrigger, TState>>(1);
        }
    }

    internal class TriggerRepresentation<TTrigger, TState>
    {
        public Func<bool> ConditionalTriggerPredicate;
        public StateRepresentation<TState, TTrigger> NextStateRepresentation;
        public object OnTriggerAction;
        public object WrappedTriggerAction;
        public readonly TTrigger Trigger;

        internal TriggerRepresentation(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Ensures(Trigger != null);

            Trigger = trigger;
        }
    }
}