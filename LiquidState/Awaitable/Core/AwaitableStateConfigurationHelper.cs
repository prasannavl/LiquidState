// Author: Prasanna V. Loganathar
// Created: 04:16 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Core;

namespace LiquidState.Awaitable.Core
{
    public class AwaitableStateConfigurationHelper<TState, TTrigger>
    {
        private readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config;
        private readonly AwaitableStateRepresentation<TState, TTrigger> currentAwaitableStateRepresentation;

        internal AwaitableStateConfigurationHelper(
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config,
            TState currentState)
        {
            Contract.Requires(config != null);
            Contract.Requires(currentState != null);

            Contract.Ensures(currentAwaitableStateRepresentation != null);

            this.config = config;
            currentAwaitableStateRepresentation = FindOrCreateStateRepresentation(currentState, config);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> OnEntry(Action action)
        {
            currentAwaitableStateRepresentation.OnEntryAction = action;
            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> OnEntry(Func<Task> asyncAction)
        {
            currentAwaitableStateRepresentation.OnEntryAction = asyncAction;
            currentAwaitableStateRepresentation.AwaitableTransitionFlags |= AwaitableTransitionFlag.EntryReturnsTask;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> OnExit(Action action)
        {
            currentAwaitableStateRepresentation.OnExitAction = action;
            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> OnExit(Func<Task> asyncAction)
        {
            currentAwaitableStateRepresentation.OnExitAction = asyncAction;
            currentAwaitableStateRepresentation.AwaitableTransitionFlags |= AwaitableTransitionFlag.ExitReturnsTask;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentAwaitableStateRepresentation.State, null);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentAwaitableStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentAwaitableStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentAwaitableStateRepresentation.State,
                onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentAwaitableStateRepresentation.State,
                onEntryAsyncAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentAwaitableStateRepresentation.State, null);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentAwaitableStateRepresentation.State, null);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentAwaitableStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentAwaitableStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentAwaitableStateRepresentation.State,
                onEntryAction);
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentAwaitableStateRepresentation.State,
                onEntryAction);
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

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.DynamicState;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask |
                                            AwaitableTransitionFlag.DynamicState;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.DynamicState;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask |
                                            AwaitableTransitionFlag.DynamicState;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask |
                                            AwaitableTransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        public AwaitableStateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask |
                                            AwaitableTransitionFlag.DynamicStateReturnsTask;

            return this;
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

        internal static bool CheckFlag(AwaitableTransitionFlag source, AwaitableTransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }

        internal static AwaitableStateRepresentation<TState, TTrigger> CreateStateRepresentation(TState state,
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config)
        {
            var rep = new AwaitableStateRepresentation<TState, TTrigger>(state);
            config[state] = rep;
            return rep;
        }

        internal static AwaitableTriggerRepresentation<TTrigger, TState> FindOrCreateTriggerRepresentation(
            TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> awaitableStateRepresentation)
        {
            Contract.Requires(awaitableStateRepresentation != null);
            Contract.Requires(trigger != null);

            Contract.Ensures(Contract.Result<AwaitableTriggerRepresentation<TTrigger, TState>>() != null);

            var rep = FindTriggerRepresentation(trigger, awaitableStateRepresentation);
            return rep ?? CreateTriggerRepresentation(trigger, awaitableStateRepresentation);
        }

        internal static AwaitableTriggerRepresentation<TTrigger, TState> CreateTriggerRepresentation(TTrigger trigger,
            AwaitableStateRepresentation<TState, TTrigger> awaitableStateRepresentation)
        {
            var rep = new AwaitableTriggerRepresentation<TTrigger, TState>(trigger);
            awaitableStateRepresentation.Triggers.Add(rep);
            return rep;
        }

        internal static AwaitableTriggerRepresentation<TTrigger, TState> FindTriggerRepresentation(TTrigger trigger,
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

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalSync(Func<bool> predicate,
            TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);

            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
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

            if (FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
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

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalAsync<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalTriggerAsync(Func<bool> predicate,
            TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalTriggerAsync<TArgument>(
            Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalPredicateAsync(
            Func<Task<bool>> predicate,
            TTrigger trigger,
            TState resultingState, Action onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> PermitInternalPredicateAsync<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> IgnoreInternal(Func<bool> predicate,
            TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = null;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private AwaitableStateConfigurationHelper<TState, TTrigger> IgnoreInternalPredicateAsync(
            Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            if (FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = null;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }
    }
}
