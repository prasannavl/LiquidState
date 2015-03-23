// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LiquidState.Common;
using LiquidState.Representations;

namespace LiquidState.Configuration
{
    public class StateMachineConfiguration<TState, TTrigger>
    {
        internal Dictionary<TState, StateRepresentation<TState, TTrigger>> Config;

        internal StateMachineConfiguration(int statesConfigStoreInitalCapacity = 4)
        {
            Config = new Dictionary<TState, StateRepresentation<TState, TTrigger>>(statesConfigStoreInitalCapacity);
        }

        internal StateMachineConfiguration(Dictionary<TState, StateRepresentation<TState, TTrigger>> config)
        {
            Config = config;
        }

        public StateConfigurationHelper<TState, TTrigger> ForState(TState state)
        {
            Contract.Requires<ArgumentNullException>(state != null);

            return new StateConfigurationHelper<TState, TTrigger>(Config, state);
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
            if (Config.TryGetValue(initialState, out rep))
            {
                return rep;
            }
            return Config.Values.FirstOrDefault();
        }
    }
}
