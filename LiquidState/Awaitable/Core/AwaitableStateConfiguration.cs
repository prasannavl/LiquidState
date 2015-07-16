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
    public class AwaitableStateConfiguration<TState, TTrigger>
    {
        private readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config;
        private readonly AwaitableStateRepresentation<TState, TTrigger> currentAwaitableStateRepresentation;

        internal AwaitableStateConfiguration(
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> config,
            TState currentState)
        {
            Contract.Requires(config != null);
            Contract.Requires(currentState != null);

            Contract.Ensures(currentAwaitableStateRepresentation != null);

            this.config = config;
            currentAwaitableStateRepresentation = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(currentState, config);
        }

        public AwaitableStateConfiguration<TState, TTrigger> OnEntry(Action action)
        {
            currentAwaitableStateRepresentation.OnEntryAction = action;
            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> OnEntry(Func<Task> asyncAction)
        {
            currentAwaitableStateRepresentation.OnEntryAction = asyncAction;
            currentAwaitableStateRepresentation.AwaitableTransitionFlags |= AwaitableTransitionFlag.EntryReturnsTask;

            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> OnExit(Action action)
        {
            currentAwaitableStateRepresentation.OnExitAction = action;
            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> OnExit(Func<Task> asyncAction)
        {
            currentAwaitableStateRepresentation.OnExitAction = asyncAction;
            currentAwaitableStateRepresentation.AwaitableTransitionFlags |= AwaitableTransitionFlag.ExitReturnsTask;

            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentAwaitableStateRepresentation.State, null);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentAwaitableStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentAwaitableStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentAwaitableStateRepresentation.State,
                onEntryAsyncAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentAwaitableStateRepresentation.State,
                onEntryAsyncAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentAwaitableStateRepresentation.State, null);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentAwaitableStateRepresentation.State, null);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentAwaitableStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentAwaitableStateRepresentation.State, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentAwaitableStateRepresentation.State,
                onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentAwaitableStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentAwaitableStateRepresentation.State,
                onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, null);
        }

        public AwaitableStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(null, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(null, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, null);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, null);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, onEntryAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public AwaitableStateConfiguration<TState, TTrigger> Ignore(TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(null, trigger);
        }

        public AwaitableStateConfiguration<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(predicate, trigger);
        }

        public AwaitableStateConfiguration<TState, TTrigger> IgnoreIf(Func<Task<bool>> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternalPredicateAsync(predicate, trigger);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.DynamicState;

            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask |
                                            AwaitableTransitionFlag.DynamicState;

            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.DynamicState;

            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask |
                                            AwaitableTransitionFlag.DynamicState;

            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask |
                                            AwaitableTransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStateFunc != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStateFunc;
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = null;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask |
                                            AwaitableTransitionFlag.DynamicStateReturnsTask;

            return this;
        }



        private AwaitableStateConfiguration<TState, TTrigger> PermitInternalSync(Func<bool> predicate,
            TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);

            rep.NextStateRepresentationWrapper = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private AwaitableStateConfiguration<TState, TTrigger> PermitInternalSync<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private AwaitableStateConfiguration<TState, TTrigger> PermitInternalAsync(Func<Task<bool>> predicate,
            TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfiguration<TState, TTrigger> PermitInternalAsync<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfiguration<TState, TTrigger> PermitInternalTriggerAsync(Func<bool> predicate,
            TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfiguration<TState, TTrigger> PermitInternalTriggerAsync<TArgument>(
            Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private AwaitableStateConfiguration<TState, TTrigger> PermitInternalPredicateAsync(
            Func<Task<bool>> predicate,
            TTrigger trigger,
            TState resultingState, Action onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private AwaitableStateConfiguration<TState, TTrigger> PermitInternalPredicateAsync<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger.Trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private AwaitableStateConfiguration<TState, TTrigger> IgnoreInternal(Func<bool> predicate,
            TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = null;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private AwaitableStateConfiguration<TState, TTrigger> IgnoreInternalPredicateAsync(
            Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            if (AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger, currentAwaitableStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = AwaitableStateConfigurationHelper.CreateTriggerRepresentation(trigger, currentAwaitableStateRepresentation);
            rep.NextStateRepresentationWrapper = null;
            rep.ConditionalTriggerPredicate = predicate;
            rep.AwaitableTransitionFlags |= AwaitableTransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }
    }
}
