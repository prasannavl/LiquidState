// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using LiquidState.Common;
using LiquidState.Machines;
using LiquidState.Representations;

namespace LiquidState.Configuration
{
    public class AsyncStateConfigurationHelper<TState, TTrigger>
    {
        private readonly Dictionary<TState, AsyncStateRepresentation<TState, TTrigger>> config;
        private readonly AsyncStateRepresentation<TState, TTrigger> currentStateRepresentation;

        internal AsyncStateConfigurationHelper(Dictionary<TState, AsyncStateRepresentation<TState, TTrigger>> config,
            TState currentState)
        {
            Contract.Requires(config != null);
            Contract.Requires(currentState != null);

            Contract.Ensures(currentStateRepresentation != null);

            this.config = config;
            currentStateRepresentation = FindOrCreateStateRepresentation(currentState, config);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> OnEntry(Action action)
        {
            currentStateRepresentation.OnEntryAction = action;
            return this;
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> OnEntry(Func<Task> asyncAction)
        {
            currentStateRepresentation.OnEntryAction = asyncAction;
            currentStateRepresentation.TransitionFlags |= AsyncStateTransitionFlag.EntryReturnsTask;

            return this;
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> OnExit(Action action)
        {
            currentStateRepresentation.OnExitAction = action;
            return this;
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> OnExit(Func<Task> asyncAction)
        {
            currentStateRepresentation.OnExitAction = asyncAction;
            currentStateRepresentation.TransitionFlags |= AsyncStateTransitionFlag.ExitReturnsTask;

            return this;
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentStateRepresentation.State, null);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentStateRepresentation.State, onEntryAsyncAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentStateRepresentation.State, onEntryAsyncAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentStateRepresentation.State, null);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentStateRepresentation.State, null);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, null);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(null, trigger, resultingState, onEntryAsyncAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(null, trigger, resultingState, onEntryAsyncAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, null);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, null);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, onEntryAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> Ignore(TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(null, trigger);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(predicate, trigger);
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> IgnoreIf(Func<Task<bool>> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternalPredicateAsync(predicate, trigger);
        }

        private AsyncStateConfigurationHelper<TState, TTrigger> PermitInternalSync(Func<bool> predicate,
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

        private AsyncStateConfigurationHelper<TState, TTrigger> PermitInternalSync<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            Contract.Assume(trigger.Trigger != null);

            var rep = FindOrCreateTriggerConfig(trigger.Trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.WrappedTriggerAction = new Action<object>(o => onEntryAction((TArgument) o));
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private AsyncStateConfigurationHelper<TState, TTrigger> PermitInternalAsync(Func<Task<bool>> predicate,
            TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AsyncStateTransitionFlag.TriggerPredicateReturnsTask;
            rep.TransitionFlags |= AsyncStateTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AsyncStateConfigurationHelper<TState, TTrigger> PermitInternalAsync<TArgument>(
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
            rep.WrappedTriggerAction = new Func<object, Task>(o => onEntryAsyncAction((TArgument) o));
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AsyncStateTransitionFlag.TriggerPredicateReturnsTask;
            rep.TransitionFlags |= AsyncStateTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AsyncStateConfigurationHelper<TState, TTrigger> PermitInternalTriggerAsync(Func<bool> predicate,
            TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AsyncStateTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AsyncStateConfigurationHelper<TState, TTrigger> PermitInternalTriggerAsync<TArgument>(
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
            rep.WrappedTriggerAction = new Func<object, Task>(o => onEntryAsyncAction((TArgument) o));
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AsyncStateTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AsyncStateConfigurationHelper<TState, TTrigger> PermitInternalPredicateAsync(Func<Task<bool>> predicate,
            TTrigger trigger,
            TState resultingState, Action onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AsyncStateTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private AsyncStateConfigurationHelper<TState, TTrigger> PermitInternalPredicateAsync<TArgument>(
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
            rep.WrappedTriggerAction = new Action<object>(o => onEntryAsyncAction((TArgument) o));
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AsyncStateTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private AsyncStateConfigurationHelper<TState, TTrigger> IgnoreInternal(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = null;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private AsyncStateConfigurationHelper<TState, TTrigger> IgnoreInternalPredicateAsync(Func<Task<bool>> predicate, TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            var rep = FindOrCreateTriggerConfig(trigger, currentStateRepresentation);

            rep.NextStateRepresentation = null;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= AsyncStateTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        internal static AsyncStateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation(TState state,
            Dictionary<TState, AsyncStateRepresentation<TState, TTrigger>> config)
        {
            Contract.Requires(state != null);
            Contract.Requires(config != null);

            Contract.Ensures(Contract.Result<AsyncStateRepresentation<TState, TTrigger>>() != null);

            AsyncStateRepresentation<TState, TTrigger> rep;
            if (config.TryGetValue(state, out rep))
            {
                if (rep != null) return rep;
            }
            rep = new AsyncStateRepresentation<TState, TTrigger>(state);

            config[state] = rep;

            return rep;
        }

        internal static AsyncTriggerRepresentation<TTrigger, TState> FindOrCreateTriggerConfig(TTrigger trigger,
            AsyncStateRepresentation<TState, TTrigger> stateRepresentation)
        {
            Contract.Requires(stateRepresentation != null);
            Contract.Requires(trigger != null);

            Contract.Ensures(Contract.Result<AsyncTriggerRepresentation<TTrigger, TState>>() != null);

            var rep = FindTriggerRepresentation(trigger, stateRepresentation);
            if (rep != null) return rep;

            rep = new AsyncTriggerRepresentation<TTrigger, TState>(trigger);
            stateRepresentation.Triggers.Add(rep);
            return rep;
        }

        internal static AsyncTriggerRepresentation<TTrigger, TState> FindTriggerRepresentation(TTrigger trigger,
            AsyncStateRepresentation<TState, TTrigger> stateRepresentation)
        {
            return stateRepresentation.Triggers.Find(x => x.Trigger.Equals(trigger));
        }
    }

    public class AsyncStateMachineConfiguration<TState, TTrigger>
    {
        internal readonly Dictionary<TState, AsyncStateRepresentation<TState, TTrigger>> config;

        internal AsyncStateMachineConfiguration(int statesConfigStoreInitalCapacity = 4)
        {
            Contract.Ensures(config != null);

            config = new Dictionary<TState, AsyncStateRepresentation<TState, TTrigger>>(statesConfigStoreInitalCapacity);
        }

        internal AsyncStateMachineConfiguration(AsyncStateMachine<TState, TTrigger> existingMachine)
        {
            Contract.Requires(existingMachine != null);
            Contract.Ensures(config != null);

            config = new Dictionary<TState, AsyncStateRepresentation<TState, TTrigger>>();
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

        internal AsyncStateRepresentation<TState, TTrigger> GetStateRepresentation(TState initialState)
        {
            Contract.Requires(initialState != null);

            AsyncStateRepresentation<TState, TTrigger> rep;
            if (config.TryGetValue(initialState, out rep))
            {
                return rep;
            }
            return config.Values.FirstOrDefault();
        }

        public AsyncStateConfigurationHelper<TState, TTrigger> Configure(TState state)
        {
            Contract.Requires<ArgumentNullException>(state != null);

            return new AsyncStateConfigurationHelper<TState, TTrigger>(config, state);
        }

        public ParameterizedTrigger<TTrigger, TArgument> SetTriggerParameter<TArgument>(TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            return new ParameterizedTrigger<TTrigger, TArgument>(trigger);
        }
    }
}