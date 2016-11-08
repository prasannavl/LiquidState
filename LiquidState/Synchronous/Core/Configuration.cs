// Author: Prasanna V. Loganathar
// Created: 04:16 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Synchronous.Core
{
    public class Configuration<TState, TTrigger>
    {
        internal Dictionary<TState, StateRepresentation<TState, TTrigger>> Representations;

        internal Configuration(int statesConfigStoreInitalCapacity = 4)
        {
            Representations =
                new Dictionary<TState, StateRepresentation<TState, TTrigger>>(statesConfigStoreInitalCapacity);
        }

        internal Configuration(Dictionary<TState, StateRepresentation<TState, TTrigger>> representations)
        {
            Representations = representations;
        }

        public StateConfiguration<TState, TTrigger> ForState(TState state)
        {
            return new StateConfiguration<TState, TTrigger>(Representations, state);
        }

        public ParameterizedTrigger<TTrigger, TArgument> SetTriggerParameter<TArgument>(TTrigger trigger)
        {
            Contract.NotNull(trigger != null, nameof(trigger));

            return new ParameterizedTrigger<TTrigger, TArgument>(trigger);
        }
    }
}