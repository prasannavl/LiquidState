// Author: Prasanna V. Loganathar
// Created: 3:36 PM 07-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Synchronous.Core
{
    public class StateConfigurationHelper<TState, TTrigger>
    {
        private readonly Dictionary<TState, StateRepresentation<TState, TTrigger>> config;
        private readonly StateRepresentation<TState, TTrigger> currentStateRepresentation;

        internal StateConfigurationHelper(Dictionary<TState, StateRepresentation<TState, TTrigger>> config,
            TState currentState)
        {
            Contract.Requires(config != null);
            Contract.Requires(currentState != null);

            Contract.Ensures(this.config != null);
            Contract.Ensures(currentStateRepresentation != null);


            this.config = config;
            currentStateRepresentation = FindOrCreateStateRepresentation(currentState, config);
        }

        public StateConfigurationHelper<TState, TTrigger> OnEntry(Action action)
        {
            currentStateRepresentation.OnEntryAction = t => action();
            return this;
        }

        public StateConfigurationHelper<TState, TTrigger> OnEntry(Action<Transition<TState, TTrigger>> action)
        {
            currentStateRepresentation.OnEntryAction = action;
            return this;
        }

        public StateConfigurationHelper<TState, TTrigger> OnExit(Action action)
        {
            currentStateRepresentation.OnExitAction = t => action();
            return this;
        }

        public StateConfigurationHelper<TState, TTrigger> OnExit(Action<Transition<TState, TTrigger>> action)
        {
            currentStateRepresentation.OnExitAction = action;
            return this;
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(null, trigger, currentStateRepresentation.State, null);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(null, trigger, currentStateRepresentation.State, t => onEntryAction());
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(null, trigger, currentStateRepresentation.State, (t, a) => onEntryAction(a));
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(predicate, trigger, currentStateRepresentation.State, null);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(predicate, trigger, currentStateRepresentation.State, t => onEntryAction());
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(predicate, trigger, currentStateRepresentation.State, (t, a) => onEntryAction(a));
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternal(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(null, trigger, resultingState, null);
        }

        public StateConfigurationHelper<TState, TTrigger> Ignore(TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(null, trigger);
        }

        public StateConfigurationHelper<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(predicate, trigger);
        }

        public StateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(null, trigger, resultingState, t => onEntryAction());
        }

        public StateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(null, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(null, trigger, resultingState, (t, a) => onEntryAction
                (a));
        }

        public StateConfigurationHelper<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(null, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(predicate, trigger, resultingState, null);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(predicate, trigger, resultingState, t => onEntryAction());
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(predicate, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(predicate, trigger, resultingState, (t, a) => onEntryAction(a));
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternal(predicate, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStatePredicate != null);

            return PermitDynamicInternal(trigger, targetStatePredicate, t => onEntryAction());
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStatePredicate != null);

            return PermitDynamicInternal(trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStatePredicate != null);

            return PermitDynamicInternal(trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStatePredicate != null);

            return PermitDynamicInternal(trigger, targetStatePredicate, (t, a) => onEntryAction(a));
        }

        internal static StateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation(TState state,
            Dictionary<TState, StateRepresentation<TState, TTrigger>> config)
        {
            Contract.Requires(state != null);
            Contract.Requires(config != null);

            Contract.Ensures(Contract.Result<StateRepresentation<TState, TTrigger>>() != null);

            StateRepresentation<TState, TTrigger> rep;
            if (config.TryGetValue(state, out rep))
            {
                return rep;
            }

            rep = CreateStateRepresentation(state, config);
            return rep;
        }

        internal static StateRepresentation<TState, TTrigger> FindStateRepresentation(TState initialState,
            Dictionary<TState, StateRepresentation<TState, TTrigger>> representations)
        {
            StateRepresentation<TState, TTrigger> rep;
            return representations.TryGetValue(initialState, out rep) ? rep : null;
        }

        internal static StateRepresentation<TState, TTrigger> CreateStateRepresentation(TState state,
            Dictionary<TState, StateRepresentation<TState, TTrigger>> config)
        {
            var rep = new StateRepresentation<TState, TTrigger>(state);
            config[state] = rep;
            return rep;
        }

        internal static TriggerRepresentation<TTrigger, TState> FindOrCreateTriggerRepresentation(TTrigger trigger,
            StateRepresentation<TState, TTrigger> stateRepresentation)
        {
            Contract.Requires(stateRepresentation != null);
            Contract.Requires(trigger != null);

            Contract.Ensures(Contract.Result<TriggerRepresentation<TTrigger, TState>>() != null);

            var rep = FindTriggerRepresentation(trigger, stateRepresentation);
            return rep ?? CreateTriggerRepresentation(trigger, stateRepresentation);
        }

        internal static TriggerRepresentation<TTrigger, TState> CreateTriggerRepresentation(TTrigger trigger,
            StateRepresentation<TState, TTrigger> stateRepresentation)
        {
            var rep = new TriggerRepresentation<TTrigger, TState>(trigger);
            stateRepresentation.Triggers.Add(rep);
            return rep;
        }

        internal static TriggerRepresentation<TTrigger, TState> FindTriggerRepresentation(TTrigger trigger,
            StateRepresentation<TState, TTrigger> stateRepresentation)
        {
            return stateRepresentation.Triggers.Find(x => x.Trigger.Equals(trigger));
        }

        internal static bool CheckFlag(TransitionFlag source, TransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitInternal(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitInternal<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternal(TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.TransitionFlags |= TransitionFlag.DynamicState;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternal<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.TransitionFlags |= TransitionFlag.DynamicState;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> IgnoreInternal(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationWrapper = null;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }
    }
}
