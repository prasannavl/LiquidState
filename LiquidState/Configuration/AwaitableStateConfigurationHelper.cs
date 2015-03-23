// Author: Prasanna V. Loganathar
// Created: 3:32 PM 07-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Common;
using LiquidState.Representations;

namespace LiquidState.Configuration
{
    public class AwaitableStateConfigurationHelper<TState, TTrigger>
    {
        private readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config;
        private readonly AwaitableStateRepresentation<TState, TTrigger> currentStateRepresentation;

        internal AwaitableStateConfigurationHelper(
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config,
            TState currentState)
        {
            Contract.Requires(config != null);
            Contract.Requires(currentState != null);

            Contract.Ensures(currentStateRepresentation != null);

            this.config = config;
            currentStateRepresentation = FindOrCreateStateRepresentation(currentState, config);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> OnEntry(Action action)
        {
            currentStateRepresentation.OnEntryAction = action;
            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> OnEntry(Func<Task> asyncAction)
        {
            currentStateRepresentation.OnEntryAction = asyncAction;
            currentStateRepresentation.TransitionFlags |= AwaitableStateTransitionFlag.EntryReturnsTask;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> OnExit(Action action)
        {
            currentStateRepresentation.OnExitAction = action;
            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> OnExit(Func<Task> asyncAction)
        {
            currentStateRepresentation.OnExitAction = asyncAction;
            currentStateRepresentation.TransitionFlags |= AwaitableStateTransitionFlag.ExitReturnsTask;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentStateRepresentation.State, null);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentStateRepresentation.State, onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentStateRepresentation.State, onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentStateRepresentation.State, null);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentStateRepresentation.State, null);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, null);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(null, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(null, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, null);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, null);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> Ignore(TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(null, trigger);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(predicate, trigger);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> IgnoreIf(Func<Task<bool>> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternalPredicateAsync(predicate, trigger);
        }

        internal static AwaitableStateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation(TState state,
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

        internal static AwaitableTriggerRepresentation<TTrigger, TState> FindOrCreateTriggerConfig(TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> stateRepresentation)
        {
            Contract.Requires(stateRepresentation != null);
            Contract.Requires(trigger != null);

            Contract.Ensures(Contract.Result<AwaitableTriggerRepresentation<TTrigger, TState>>() != null);

            var rep = FindTriggerRepresentation(trigger, stateRepresentation);
            if (rep != null) return rep;

            rep = new AwaitableTriggerRepresentation<TTrigger, TState>(trigger);
            stateRepresentation.Triggers.Add(rep);
            return rep;
        }

        internal static AwaitableTriggerRepresentation<TTrigger, TState> FindTriggerRepresentation(TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> stateRepresentation)
        {
            return stateRepresentation.Triggers.Find(x => x.Trigger.Equals(trigger));
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalSync(Func<bool> predicate,
            TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalSync<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            Contract.Assume(trigger.Trigger != null);

            var rep = FindOrCreateTriggerConfig(trigger.Trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalAsync(Func<Task<bool>> predicate,
            TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AwaitableStateTransitionFlag.TriggerPredicateReturnsTask;
            rep.TransitionFlags |= AwaitableStateTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalAsync<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            Contract.Assume(trigger.Trigger != null);

            var rep = FindOrCreateTriggerConfig(trigger.Trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AwaitableStateTransitionFlag.TriggerPredicateReturnsTask;
            rep.TransitionFlags |= AwaitableStateTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalTriggerAsync(Func<bool> predicate,
            TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AwaitableStateTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalTriggerAsync<TArgument>(
            Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            Contract.Assume(trigger.Trigger != null);

            var rep = FindOrCreateTriggerConfig(trigger.Trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AwaitableStateTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalPredicateAsync(
            Func<Task<bool>> predicate,
            TTrigger trigger,
            TState resultingState, Action onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AwaitableStateTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalPredicateAsync<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            Contract.Assume(trigger.Trigger != null);

            var rep = FindOrCreateTriggerConfig(trigger.Trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AwaitableStateTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> IgnoreInternal(Func<bool> predicate,
            TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = null;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> IgnoreInternalPredicateAsync(
            Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = null;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AwaitableStateTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }
    }
}
