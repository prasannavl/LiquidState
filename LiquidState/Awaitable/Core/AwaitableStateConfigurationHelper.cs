using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Awaitable.Core
{
    internal static class AwaitableStateConfigurationHelper
    {
        internal static AwaitableStateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation<TState, TTrigger>(TState state,
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config)
        {
            Contract.Requires(state != null);
            Contract.Requires(config != null);

            Contract.Ensures(Contract.Result<AwaitableStateRepresentation<TState, TTrigger>>() != null);

            AwaitableStateRepresentation<TState, TTrigger> rep;
            if (config.TryGetValue(state, out rep))
            {
                if (rep != null) return rep;
            }
            rep = new AwaitableStateRepresentation<TState, TTrigger>(state);

            config[state] = rep;

            return rep;
        }

        internal static bool CheckFlag(AwaitableTransitionFlag source, AwaitableTransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }

        internal static AwaitableStateRepresentation<TState, TTrigger> CreateStateRepresentation<TState, TTrigger>(TState state,
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config)
        {
            var rep = new AwaitableStateRepresentation<TState, TTrigger>(state);
            config[state] = rep;
            return rep;
        }

        internal static AwaitableTriggerRepresentation<TTrigger, TState> FindOrCreateTriggerRepresentation<TTrigger, TState>(
            TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> awaitableStateRepresentation)
        {
            Contract.Requires(awaitableStateRepresentation != null);
            Contract.Requires(trigger != null);

            Contract.Ensures(Contract.Result<AwaitableTriggerRepresentation<TTrigger, TState>>() != null);

            var rep = FindTriggerRepresentation(trigger, awaitableStateRepresentation);
            return rep ?? CreateTriggerRepresentation(trigger, awaitableStateRepresentation);
        }

        internal static AwaitableTriggerRepresentation<TTrigger, TState> CreateTriggerRepresentation<TTrigger, TState>(TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> awaitableStateRepresentation)
        {
            var rep = new AwaitableTriggerRepresentation<TTrigger, TState>(trigger);
            awaitableStateRepresentation.Triggers.Add(rep);
            return rep;
        }

        internal static AwaitableTriggerRepresentation<TTrigger, TState> FindTriggerRepresentation<TTrigger, TState>(TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> awaitableStateRepresentation)
        {
            return awaitableStateRepresentation.Triggers.Find(x => x.Trigger.Equals(trigger));
        }

        internal static AwaitableStateRepresentation<TState, TTrigger> FindStateRepresentation<TState, TTrigger>(
            TState initialState, Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations)
        {
            AwaitableStateRepresentation<TState, TTrigger> rep;
            return representations.TryGetValue(initialState, out rep) ? rep : null;
        }
    }
}