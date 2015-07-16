// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LiquidState.Core;

namespace LiquidState.Awaitable.Core
{
    public class AwaitableConfiguration<TState, TTrigger>
    {
        internal readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> Representations;

        internal AwaitableConfiguration(int statesConfigStoreInitalCapacity = 4)
        {
            Contract.Ensures(Representations != null);

            Representations =
                new Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>>(statesConfigStoreInitalCapacity);
        }

        internal AwaitableConfiguration(
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations)
        {
            Contract.Ensures(representations != null);

            Representations = representations;
        }

        public AwaitableStateConfiguration<TState, TTrigger> ForState(TState state)
        {
            Contract.Requires<ArgumentNullException>(state != null);

            return new AwaitableStateConfiguration<TState, TTrigger>(Representations, state);
        }

        public ParameterizedTrigger<TTrigger, TArgument> SetTriggerParameter<TArgument>(TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            return new ParameterizedTrigger<TTrigger, TArgument>(trigger);
        }

        internal AwaitableStateRepresentation<TState, TTrigger> GetInitialStateRepresentation(TState initialState)
        {
            Contract.Requires(initialState != null);

            AwaitableStateRepresentation<TState, TTrigger> rep;
            return Representations.TryGetValue(initialState, out rep) ? rep : Representations.Values.FirstOrDefault();
        }
    }
}
