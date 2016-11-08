// Author: Prasanna V. Loganathar
// Created: 04:16 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using LiquidState.Core;
using LiquidState.Common;

namespace LiquidState.Awaitable.Core
{
    public class AwaitableConfiguration<TState, TTrigger>
    {
        internal readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> Representations;

        internal AwaitableConfiguration(int statesConfigStoreInitalCapacity = 4)
        {
            Representations =
                new Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>>(statesConfigStoreInitalCapacity);
        }

        internal AwaitableConfiguration(
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations)
        {
            Representations = representations;
        }

        public AwaitableStateConfiguration<TState, TTrigger> ForState(TState state)
        {
            return new AwaitableStateConfiguration<TState, TTrigger>(Representations, state);
        }

        public ParameterizedTrigger<TTrigger, TArgument> SetTriggerParameter<TArgument>(TTrigger trigger)
        {
            Contract.NotNull(trigger != null, nameof(trigger));

            return new ParameterizedTrigger<TTrigger, TArgument>(trigger);
        }

        internal AwaitableStateRepresentation<TState, TTrigger> GetInitialStateRepresentation(TState initialState)
        {
            AwaitableStateRepresentation<TState, TTrigger> rep;
            return Representations.TryGetValue(initialState, out rep) ? rep : Representations.Values.FirstOrDefault();
        }
    }
}