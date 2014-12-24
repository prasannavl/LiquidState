using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LiquidState.Common;
using LiquidState.Representations;

namespace LiquidState.Configuration
{
    public class FluidStateConfigurationHelper<TState, TTrigger>
    {
        private readonly Dictionary<TState, FluidStateRepresentation<TState, TTrigger>> config;
        private readonly FluidStateRepresentation<TState, TTrigger> currentStateRepresentation;

        internal FluidStateConfigurationHelper(Dictionary<TState, FluidStateRepresentation<TState, TTrigger>> config,
            TState currentState)
        {
            Contract.Requires(config != null);
            Contract.Requires(currentState != null);

            Contract.Ensures(this.config != null);
            Contract.Ensures(currentStateRepresentation != null);


            this.config = config;
            currentStateRepresentation = FindOrCreateStateRepresentation(currentState, config);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> OnEntry(Action action)
        {
            currentStateRepresentation.OnEntryAction = action;
            return this;
        }

        public FluidStateConfigurationHelper<TState, TTrigger> OnExit(Action action)
        {
            currentStateRepresentation.OnExitAction = action;
            return this;
        }

        public FluidStateConfigurationHelper<TState, TTrigger> ForceFluidFlowOn()
        {
            currentStateRepresentation.TransitionFlags |= FluidTransitionFlag.FluidStateActive |
                                                          FluidTransitionFlag.OverrideFluidState;
            return this;
        }

        public FluidStateConfigurationHelper<TState, TTrigger> ForceFluidFlowOff()
        {
            currentStateRepresentation.TransitionFlags |= (~FluidTransitionFlag.FluidStateActive) |
                                                          FluidTransitionFlag.OverrideFluidState;
            return this;
        }

        public FluidStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(null, trigger, currentStateRepresentation.State, null);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(predicate, trigger, currentStateRepresentation.State, null);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(null, trigger, resultingState, null);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> Ignore(TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(null, trigger);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(predicate, trigger);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(null, trigger, resultingState, onEntryAction);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(null, trigger, resultingState, onEntryAction);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(predicate, trigger, resultingState, null);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(predicate, trigger, resultingState, onEntryAction);
        }

        public FluidStateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(predicate, trigger, resultingState, onEntryAction);
        }

        private FluidStateConfigurationHelper<TState, TTrigger> PermitInternal(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            var rep = FindOrCreateTriggerRepresentation(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private FluidStateConfigurationHelper<TState, TTrigger> PermitInternal<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            Contract.Assume(trigger.Trigger != null);

            var rep = FindOrCreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private FluidStateConfigurationHelper<TState, TTrigger> IgnoreInternal(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            var rep = FindOrCreateTriggerRepresentation(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = null;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        internal static FluidStateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation(TState state,
            Dictionary<TState, FluidStateRepresentation<TState, TTrigger>> config)
        {
            Contract.Requires(config != null);

            Contract.Ensures(Contract.Result<FluidStateRepresentation<TState, TTrigger>>() != null);

            if (state == null)
                return null;

            FluidStateRepresentation<TState, TTrigger> rep;
            if (config.TryGetValue(state, out rep))
            {
                return rep;
            }

            rep = new FluidStateRepresentation<TState, TTrigger>(state);
            config[state] = rep;

            return rep;
        }

        internal static FluidTriggerRepresentation<TTrigger, TState> FindOrCreateTriggerRepresentation(TTrigger trigger,
            FluidStateRepresentation<TState, TTrigger> stateRepresentation)
        {
            Contract.Requires(stateRepresentation != null);
            Contract.Requires(trigger != null);

            Contract.Ensures(Contract.Result<FluidTriggerRepresentation<TTrigger, TState>>() != null);

            var rep = FindTriggerRepresentation(trigger, stateRepresentation);
            if (rep != null)
            {
                Contract.Assume(rep.Trigger != null);
                return rep;
            }

            rep = new FluidTriggerRepresentation<TTrigger, TState>(trigger);
            stateRepresentation.Triggers.Add(rep);
            return rep;
        }

        internal static FluidTriggerRepresentation<TTrigger, TState> FindTriggerRepresentation(TTrigger trigger,
            FluidStateRepresentation<TState, TTrigger> stateRepresentation)
        {
            if (stateRepresentation == null || stateRepresentation.Triggers == null)
                return null;
            return stateRepresentation.Triggers.Find(x => x.Trigger.Equals(trigger));
        }

        internal static FluidTriggerRepresentation<TTrigger, TState> FindTriggerRepresentationForTargetState(TState targetState, FluidStateRepresentation<TState, TTrigger> stateRepresentation)
        {
            if (stateRepresentation == null || stateRepresentation.Triggers == null)
                return null;
            return stateRepresentation.Triggers.Find(x => x.NextStateRepresentation.State.Equals(targetState));
        }
    }
}