using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Awaitable.Core
{
    internal static class AwaitableStateConfigurationHelper
    {
        internal static AwaitableStateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation<TState, TTrigger>
            (TState state,
                Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations)
        {
            Contract.Requires(state != null);
            Contract.Requires(representations != null);

            Contract.Ensures(Contract.Result<AwaitableStateRepresentation<TState, TTrigger>>() != null);

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

        internal static AwaitableTriggerRepresentation<TTrigger, TState> FindOrCreateTriggerRepresentation
            <TTrigger, TState>(
            TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> representation)
        {
            Contract.Requires(representation != null);
            Contract.Requires(trigger != null);

            Contract.Ensures(Contract.Result<AwaitableTriggerRepresentation<TTrigger, TState>>() != null);

            var rep = FindTriggerRepresentation(trigger, representation);
            return rep ?? CreateTriggerRepresentation(trigger, representation);
        }

        internal static AwaitableTriggerRepresentation<TTrigger, TState> CreateTriggerRepresentation<TTrigger, TState>(
            TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> representation)
        {
            var rep = new AwaitableTriggerRepresentation<TTrigger, TState>(trigger);
            representation.Triggers.Add(rep);
            return rep;
        }

        internal static AwaitableTriggerRepresentation<TTrigger, TState> FindTriggerRepresentation<TTrigger, TState>(
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
