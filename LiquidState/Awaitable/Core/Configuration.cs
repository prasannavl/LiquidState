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
    public class Configuration<TState, TTrigger>
    {
        internal readonly Dictionary<TState, StateRepresentation<TState, TTrigger>> Representations;

        internal Configuration(int statesConfigStoreInitalCapacity = 4)
        {
            Contract.Ensures(Representations != null);

            Representations =
                new Dictionary<TState, StateRepresentation<TState, TTrigger>>(statesConfigStoreInitalCapacity);
        }

        internal Configuration(
            Dictionary<TState, StateRepresentation<TState, TTrigger>> representations)
        {
            Contract.Ensures(representations != null);

            Representations = representations;
        }

        public StateConfigurationHelper<TState, TTrigger> ForState(TState state)
        {
            Contract.Requires<ArgumentNullException>(state != null);

            return new StateConfigurationHelper<TState, TTrigger>(Representations, state);
        }

        public ParameterizedTrigger<TTrigger, TArgument> SetTriggerParameter<TArgument>(TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            return new ParameterizedTrigger<TTrigger, TArgument>(trigger);
        }

        internal StateRepresentation<TState, TTrigger> GetInitialStateRepresentation(TState initialState)
        {
            Contract.Requires(initialState != null);

            StateRepresentation<TState, TTrigger> rep;
            return Representations.TryGetValue(initialState, out rep) ? rep : Representations.Values.FirstOrDefault();
        }
    }
}
