// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LiquidState.Common;
using LiquidState.Machines;
using LiquidState.Representations;

namespace LiquidState.Configuration
{
    public class AwaitableStateMachineConfiguration<TState, TTrigger>
    {
        internal readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config;

        internal AwaitableStateMachineConfiguration(int statesConfigStoreInitalCapacity = 4)
        {
            Contract.Ensures(config != null);

            config = new Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>>(statesConfigStoreInitalCapacity);
        }

        internal AwaitableStateMachineConfiguration(AwaitableStateMachine<TState, TTrigger> existingMachine)
        {
            Contract.Requires(existingMachine != null);
            Contract.Ensures(config != null);

            config = new Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>>();
            var currentStateRep = existingMachine.currentStateRepresentation;
            var currentTriggers = currentStateRep.Triggers.ToArray();

            foreach (var triggerRepresentation in currentTriggers)
            {
                var nextStateRep = triggerRepresentation.NextStateRepresentation;
                if (nextStateRep != currentStateRep)
                {
                    if (!config.ContainsKey(nextStateRep.State))
                    {
                        config.Add(nextStateRep.State, nextStateRep);
                    }
                }
            }
        }

        internal AwaitableStateRepresentation<TState, TTrigger> GetStateRepresentation(TState initialState)
        {
            Contract.Requires(initialState != null);

            AwaitableStateRepresentation<TState, TTrigger> rep;
            if (config.TryGetValue(initialState, out rep))
            {
                return rep;
            }
            return config.Values.FirstOrDefault();
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> Configure(TState state)
        {
            Contract.Requires<ArgumentNullException>(state != null);

            return new AwaitableStateConfigurationHelper<TState, TTrigger>(config, state);
        }

        public ParameterizedTrigger<TTrigger, TArgument> SetTriggerParameter<TArgument>(TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            return new ParameterizedTrigger<TTrigger, TArgument>(trigger);
        }
    }
}