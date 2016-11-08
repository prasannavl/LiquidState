// Author: Prasanna V. Loganathar
// Created: 12:19 16-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.Generic;

namespace LiquidState.Awaitable.Core
{
    internal static class AwaitableStateConfigurationHelper
    {
        internal static AwaitableStateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation<TState, TTrigger>
        (TState state,
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations)
        {
            AwaitableStateRepresentation<TState, TTrigger> rep;
            if (representations.TryGetValue(state, out rep))
            {
                if (rep != null) return rep;
            }
            rep = new AwaitableStateRepresentation<TState, TTrigger>(state);

            representations[state] = rep;

            return rep;
        }

        internal static bool CheckFlag(AwaitableTransitionFlag source, AwaitableTransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }

        internal static AwaitableStateRepresentation<TState, TTrigger> CreateStateRepresentation<TState, TTrigger>(
            TState state,
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations)
        {
            var rep = new AwaitableStateRepresentation<TState, TTrigger>(state);
            representations[state] = rep;
            return rep;
        }

        internal static AwaitableTriggerRepresentation<TTrigger> FindOrCreateTriggerRepresentation
            <TTrigger, TState>(
                TTrigger trigger,
                AwaitableStateRepresentation<TState, TTrigger> representation)
        {
            var rep = FindTriggerRepresentation(trigger, representation);
            return rep ?? CreateTriggerRepresentation(trigger, representation);
        }

        internal static AwaitableTriggerRepresentation<TTrigger> CreateTriggerRepresentation<TState, TTrigger>(
            TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> representation)
        {
            var rep = new AwaitableTriggerRepresentation<TTrigger>(trigger);
            representation.Triggers.Add(rep);
            return rep;
        }

        internal static AwaitableTriggerRepresentation<TTrigger> FindTriggerRepresentation<TState, TTrigger>(
            TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> representation)
        {
            return representation.Triggers.Find(x => x.Trigger.Equals(trigger));
        }

        internal static AwaitableStateRepresentation<TState, TTrigger> FindStateRepresentation<TState, TTrigger>(
            TState initialState, Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations)
        {
            AwaitableStateRepresentation<TState, TTrigger> rep;
            return representations.TryGetValue(initialState, out rep) ? rep : null;
        }
    }
}