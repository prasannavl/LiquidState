// Author: Prasanna V. Loganathar
// Created: 11:54 16-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.Generic;

namespace LiquidState.Synchronous.Core
{
    internal static class StateConfigurationHelper
    {
        internal static StateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation<TState, TTrigger>(
            TState state,
            Dictionary<TState, StateRepresentation<TState, TTrigger>> representations)
        {
            StateRepresentation<TState, TTrigger> rep;
            if (representations.TryGetValue(state, out rep)) { return rep; }

            rep = CreateStateRepresentation(state, representations);
            return rep;
        }

        internal static StateRepresentation<TState, TTrigger> FindStateRepresentation<TState, TTrigger>(
            TState initialState,
            Dictionary<TState, StateRepresentation<TState, TTrigger>> representations)
        {
            StateRepresentation<TState, TTrigger> rep;
            return representations.TryGetValue(initialState, out rep) ? rep : null;
        }

        internal static StateRepresentation<TState, TTrigger> CreateStateRepresentation<TState, TTrigger>(TState state,
            Dictionary<TState, StateRepresentation<TState, TTrigger>> representations)
        {
            var rep = new StateRepresentation<TState, TTrigger>(state);
            representations[state] = rep;
            return rep;
        }

        internal static TriggerRepresentation<TTrigger> FindOrCreateTriggerRepresentation<TState, TTrigger>(
            TTrigger trigger,
            StateRepresentation<TState, TTrigger> representation)
        {
            var rep = FindTriggerRepresentation(trigger, representation);
            return rep ?? CreateTriggerRepresentation(trigger, representation);
        }

        internal static TriggerRepresentation<TTrigger> CreateTriggerRepresentation<TState, TTrigger>(
            TTrigger trigger,
            StateRepresentation<TState, TTrigger> representation)
        {
            var rep = new TriggerRepresentation<TTrigger>(trigger);
            representation.Triggers.Add(rep);
            return rep;
        }

        internal static TriggerRepresentation<TTrigger> FindTriggerRepresentation<TState, TTrigger>(
            TTrigger trigger,
            StateRepresentation<TState, TTrigger> representation)
        {
            return representation.Triggers.Find(x => x.Trigger.Equals(trigger));
        }

        internal static bool CheckFlag(TransitionFlag source, TransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }
    }
}