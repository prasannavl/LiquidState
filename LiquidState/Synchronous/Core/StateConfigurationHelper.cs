using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Synchronous.Core
{
    internal static class StateConfigurationHelper
    {
        internal static StateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation<TState, TTrigger>(
            TState state,
            Dictionary<TState, StateRepresentation<TState, TTrigger>> representations)
        {
            Contract.Requires(state != null);
            Contract.Requires(representations != null);

            Contract.Ensures(Contract.Result<StateRepresentation<TState, TTrigger>>() != null);

            StateRepresentation<TState, TTrigger> rep;
            if (representations.TryGetValue(state, out rep))
            {
                return rep;
            }

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

        internal static TriggerRepresentation<TTrigger, TState> FindOrCreateTriggerRepresentation<TTrigger, TState>(
            TTrigger trigger,
            StateRepresentation<TState, TTrigger> representation)
        {
            Contract.Requires(representation != null);
            Contract.Requires(trigger != null);

            Contract.Ensures(Contract.Result<TriggerRepresentation<TTrigger, TState>>() != null);

            var rep = FindTriggerRepresentation(trigger, representation);
            return rep ?? CreateTriggerRepresentation(trigger, representation);
        }

        internal static TriggerRepresentation<TTrigger, TState> CreateTriggerRepresentation<TTrigger, TState>(
            TTrigger trigger,
            StateRepresentation<TState, TTrigger> representation)
        {
            var rep = new TriggerRepresentation<TTrigger, TState>(trigger);
            representation.Triggers.Add(rep);
            return rep;
        }

        internal static TriggerRepresentation<TTrigger, TState> FindTriggerRepresentation<TTrigger, TState>(
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
