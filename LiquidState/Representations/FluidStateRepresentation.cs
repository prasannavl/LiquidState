// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Representations
{
    internal class FluidStateRepresentation<TState, TTrigger>
    {
        public Action OnEntryAction;
        public Action OnExitAction;
        public FluidTransitionFlag TransitionFlags;
        public readonly TState State;
        public readonly List<FluidTriggerRepresentation<TTrigger, TState>> Triggers;

        internal FluidStateRepresentation(TState state)
        {
            State = state;
            // Allocate with capacity as 1 to avoid wastage of memory.
            Triggers = new List<FluidTriggerRepresentation<TTrigger, TState>>(1);
        }
    }

    internal class FluidTriggerRepresentation<TTrigger, TState>
    {
        public Func<bool> ConditionalTriggerPredicate;
        public FluidStateRepresentation<TState, TTrigger> NextStateRepresentation;
        public object OnTriggerAction;
        public readonly TTrigger Trigger;

        internal FluidTriggerRepresentation(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Ensures(Trigger != null);

            Trigger = trigger;
        }
    }

    [Flags]
    internal enum FluidTransitionFlag : byte
    {
        None = AwaitableStateTransitionFlag.None,
        OverrideFluidState = AwaitableStateTransitionFlag.OverrideFluidState,
        FluidStateActive = AwaitableStateTransitionFlag.FluidStateActive,
    }
}