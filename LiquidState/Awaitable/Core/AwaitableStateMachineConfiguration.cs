// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LiquidState.Configuration;
using LiquidState.Core;
using LiquidState.Representations;

namespace LiquidState.Awaitable.Core
{
    public class AwaitableStateMachineConfiguration<TState, TTrigger>
    {
        internal readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> Config;

        internal AwaitableStateMachineConfiguration(int statesConfigStoreInitalCapacity = 4)
        {
            Contract.Ensures(Config != null);

            Config =
                new Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>>(statesConfigStoreInitalCapacity);
        }

        internal AwaitableStateMachineConfiguration(
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config)
        {
            Contract.Ensures(config != null);

            Config = config;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> ForState(TState state)
        {
            Contract.Requires<ArgumentNullException>(state != null);

            return new AwaitableStateConfigurationHelper<TState, TTrigger>(Config, state);
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
            if (Config.TryGetValue(initialState, out rep))
            {
                return rep;
            }
            return Config.Values.FirstOrDefault();
        }
    }
}
