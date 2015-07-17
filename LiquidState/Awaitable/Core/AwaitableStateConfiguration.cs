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
        internal readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> Representations;
        internal readonly AwaitableStateRepresentation<TState, TTrigger> CurrentStateRepresentation;

        internal AwaitableStateConfiguration(
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations,
            TState currentState)
        {
            Contract.Requires(representations != null);
            Contract.Requires(currentState != null);

            Contract.Ensures(CurrentStateRepresentation != null);

            Representations = representations;
            CurrentStateRepresentation = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(
                currentState, representations);
        }

        #region Entry and Exit

        public AwaitableStateConfiguration<TState, TTrigger> OnEntry(Action<Transition<TState, TTrigger>> action)
        {
            return AwaitableStateConfigurationMethodHelper.OnEntry(this, action, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> OnEntry(Func<Transition<TState, TTrigger>, Task> action)
        {
            return AwaitableStateConfigurationMethodHelper.OnEntry(this, action,
                AwaitableTransitionFlag.EntryReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> OnExit(Action<Transition<TState, TTrigger>> action)
        {
            return AwaitableStateConfigurationMethodHelper.OnExit(this, action, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> OnExit(Func<Transition<TState, TTrigger>, Task> action)
        {
            return AwaitableStateConfigurationMethodHelper.OnExit(this, action, AwaitableTransitionFlag.ExitReturnsTask);
        }

        #endregion

        #region Permit

        public AwaitableStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger, resultingState, null,
                AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger, resultingState,
                onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger.Trigger, resultingState,
                onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger.Trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }

        #endregion

        #region PermitReentry

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger,
                CurrentStateRepresentation.State, null, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger,
                CurrentStateRepresentation.State, onTriggerAction, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger,
            Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger,
                CurrentStateRepresentation.State,
                onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }

        #region Generic Variants

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger.Trigger,
                CurrentStateRepresentation.State, onTriggerAction, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return PermitReentry(trigger, (t, a) => onTriggerAction(a));
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, null, trigger.Trigger,
                CurrentStateRepresentation.State,
                onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TArgument, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return PermitReentry(trigger, (t, a) => onTriggerAction(a));
        }

        #endregion

        #endregion

        #region PermitReentryIf

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger,
                CurrentStateRepresentation.State, null, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger,
                CurrentStateRepresentation.State, null, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger,
                CurrentStateRepresentation.State, onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger.Trigger,
                CurrentStateRepresentation.State, onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger, Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger,
                CurrentStateRepresentation.State,
                onTriggerAction, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger.Trigger,
                CurrentStateRepresentation.State,
                onTriggerAction, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }

        #endregion

        #region PermitIf

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger, resultingState,
                null, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger,
                resultingState, null, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger, resultingState,
                onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger, resultingState,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerPredicateReturnsTask | AwaitableTransitionFlag.TriggerActionReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger.Trigger, resultingState,
                onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger.Trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger.Trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return AwaitableStateConfigurationMethodHelper.Permit(this, predicate, trigger.Trigger, resultingState,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerPredicateReturnsTask | AwaitableTransitionFlag.TriggerActionReturnsTask);
        }

        #endregion

        #region Ignore & IngoreIf

        public AwaitableStateConfiguration<TState, TTrigger> Ignore(TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return AwaitableStateConfigurationMethodHelper.Ignore(this, null, trigger, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return AwaitableStateConfigurationMethodHelper.Ignore(this, predicate, trigger, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> IgnoreIf(Func<Task<bool>> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return AwaitableStateConfigurationMethodHelper.Ignore(this, predicate, trigger,
                AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }

        #endregion

        #region Dynamic

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(
            TTrigger trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStateFunc != null);

            return AwaitableStateConfigurationMethodHelper.PermitDynamic(this, trigger, targetStateFunc,
                onTriggerAction, AwaitableTransitionFlag.DynamicState);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(
            TTrigger trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStateFunc != null);

            return AwaitableStateConfigurationMethodHelper.PermitDynamic(this, trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerActionReturnsTask |
                AwaitableTransitionFlag.DynamicState);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(
            TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStateFunc != null);

            return AwaitableStateConfigurationMethodHelper.PermitDynamic(this, trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.DynamicStateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(
            TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStateFunc != null);

            return AwaitableStateConfigurationMethodHelper.PermitDynamic(this, trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerActionReturnsTask |
                AwaitableTransitionFlag.DynamicStateReturnsTask);
        }

        #region Generic Variants

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStateFunc != null);

            return AwaitableStateConfigurationMethodHelper.PermitDynamic(this, trigger.Trigger, targetStateFunc,
                onTriggerAction, AwaitableTransitionFlag.DynamicState);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStateFunc != null);

            return AwaitableStateConfigurationMethodHelper.PermitDynamic(this, trigger.Trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.DynamicStateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStateFunc != null);

            return AwaitableStateConfigurationMethodHelper.PermitDynamic(this, trigger.Trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerActionReturnsTask |
                AwaitableTransitionFlag.DynamicState);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStateFunc != null);

            return AwaitableStateConfigurationMethodHelper.PermitDynamic(this, trigger.Trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerActionReturnsTask |
                AwaitableTransitionFlag.DynamicStateReturnsTask);
        }

        #endregion

        #endregion
    }
}
