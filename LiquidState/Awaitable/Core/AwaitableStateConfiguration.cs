// Author: Prasanna V. Loganathar
// Created: 03:37 17-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiquidState.Core;
using Helper = LiquidState.Awaitable.Core.AwaitableStateConfigurationMethodHelper;

namespace LiquidState.Awaitable.Core
{
    public class AwaitableStateConfiguration<TState, TTrigger>
    {
        internal readonly AwaitableStateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        internal readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> Representations;

        internal AwaitableStateConfiguration(
            Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations,
            TState currentState)
        {
            Representations = representations;
            CurrentStateRepresentation = AwaitableStateConfigurationHelper.FindOrCreateStateRepresentation(
                currentState, representations);
        }

        #region Entry and Exit

        public AwaitableStateConfiguration<TState, TTrigger> OnEntry(Action<Transition<TState, TTrigger>> action)
        {
            return Helper.OnEntry(this, action, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> OnEntry(Func<Transition<TState, TTrigger>, Task> action)
        {
            return Helper.OnEntry(this, action,
                AwaitableTransitionFlag.EntryReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> OnExit(Action<Transition<TState, TTrigger>> action)
        {
            return Helper.OnExit(this, action, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> OnExit(Func<Transition<TState, TTrigger>, Task> action)
        {
            return Helper.OnExit(this, action, AwaitableTransitionFlag.ExitReturnsTask);
        }

        #endregion

        #region Permit

        public AwaitableStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            return Helper.Permit(this, null, trigger, resultingState, null,
                AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return Helper.Permit(this, null, trigger, resultingState,
                onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            return Helper.Permit(this, null, trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            return Helper.Permit(this, null, trigger.Trigger, resultingState,
                onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            return Helper.Permit(this, null, trigger.Trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }

        #endregion

        #region PermitReentry

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            return Helper.Permit(this, null, trigger,
                CurrentStateRepresentation.State, null, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return Helper.Permit(this, null, trigger,
                CurrentStateRepresentation.State, onTriggerAction, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger,
            Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            return Helper.Permit(this, null, trigger,
                CurrentStateRepresentation.State,
                onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }

        #region Generic Variants

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            return Helper.Permit(this, null, trigger.Trigger,
                CurrentStateRepresentation.State, onTriggerAction, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onTriggerAction)
        {
            return PermitReentry(trigger, (t, a) => onTriggerAction(a));
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            return Helper.Permit(this, null, trigger.Trigger,
                CurrentStateRepresentation.State,
                onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TArgument, Task> onTriggerAction)
        {
            return PermitReentry(trigger, (t, a) => onTriggerAction(a));
        }

        #endregion

        #endregion

        #region PermitReentryIf

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger)
        {
            return Helper.Permit(this, predicate, trigger,
                CurrentStateRepresentation.State, null, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            return Helper.Permit(this, predicate, trigger,
                CurrentStateRepresentation.State, null, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger,
                CurrentStateRepresentation.State, onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger.Trigger,
                CurrentStateRepresentation.State, onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger, Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger,
                CurrentStateRepresentation.State,
                onTriggerAction, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger.Trigger,
                CurrentStateRepresentation.State,
                onTriggerAction, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }

        #endregion

        #region PermitIf

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            return Helper.Permit(this, predicate, trigger, resultingState,
                null, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState)
        {
            return Helper.Permit(this, predicate, trigger,
                resultingState, null, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }

        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger, resultingState,
                onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger, resultingState,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerPredicateReturnsTask | AwaitableTransitionFlag.TriggerActionReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger.Trigger, resultingState,
                onTriggerAction, AwaitableTransitionFlag.None);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger.Trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger.Trigger,
                resultingState, onTriggerAction, AwaitableTransitionFlag.TriggerActionReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            return Helper.Permit(this, predicate, trigger.Trigger, resultingState,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerPredicateReturnsTask | AwaitableTransitionFlag.TriggerActionReturnsTask);
        }

        #endregion

        #region Ignore & IngoreIf

        public AwaitableStateConfiguration<TState, TTrigger> Ignore(TTrigger trigger)
        {
            return Helper.Ignore(this, null, trigger, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            return Helper.Ignore(this, predicate, trigger, AwaitableTransitionFlag.None);
        }

        public AwaitableStateConfiguration<TState, TTrigger> IgnoreIf(Func<Task<bool>> predicate, TTrigger trigger)
        {
            return Helper.Ignore(this, predicate, trigger,
                AwaitableTransitionFlag.TriggerPredicateReturnsTask);
        }

        #endregion

        #region Dynamic

        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(
            TTrigger trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return Helper.PermitDynamic(this, trigger, targetStateFunc,
                onTriggerAction, AwaitableTransitionFlag.DynamicState);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(
            TTrigger trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            return Helper.PermitDynamic(this, trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerActionReturnsTask |
                AwaitableTransitionFlag.DynamicState);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(
            TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return Helper.PermitDynamic(this, trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.DynamicStateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic(
            TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<Transition<TState, TTrigger>, Task> onTriggerAction)
        {
            return Helper.PermitDynamic(this, trigger, targetStateFunc,
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
            return Helper.PermitDynamic(this, trigger.Trigger, targetStateFunc,
                onTriggerAction, AwaitableTransitionFlag.DynamicState);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            return Helper.PermitDynamic(this, trigger.Trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.DynamicStateReturnsTask);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            return Helper.PermitDynamic(this, trigger.Trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerActionReturnsTask |
                AwaitableTransitionFlag.DynamicState);
        }


        public AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<Transition<TState, TTrigger>, TArgument, Task> onTriggerAction)
        {
            return Helper.PermitDynamic(this, trigger.Trigger, targetStateFunc,
                onTriggerAction,
                AwaitableTransitionFlag.TriggerActionReturnsTask |
                AwaitableTransitionFlag.DynamicStateReturnsTask);
        }

        #endregion

        #endregion
    }
}