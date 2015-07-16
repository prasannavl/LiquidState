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
    public class StateConfiguration<TState, TTrigger>
    {
        internal readonly Dictionary<TState, StateRepresentation<TState, TTrigger>> Representations;
        internal readonly StateRepresentation<TState, TTrigger> CurrentStateRepresentation;

        internal StateConfiguration(Dictionary<TState, StateRepresentation<TState, TTrigger>> representations,
            TState currentState)
        {
            Contract.Requires(representations != null);
            Contract.Requires(currentState != null);

            Contract.Ensures(Representations != null);
            Contract.Ensures(CurrentStateRepresentation != null);


            Representations = representations;
            CurrentStateRepresentation = StateConfigurationHelper.FindOrCreateStateRepresentation(currentState,
                representations);
        }

        #region Entry and Exit

        public StateConfiguration<TState, TTrigger> OnEntry(Action action)
        {
            return StateConfigurationMethodHelper.OnEntry(this, t => action());
        }

        public StateConfiguration<TState, TTrigger> OnEntry(Action<Transition<TState, TTrigger>> action)
        {
            return StateConfigurationMethodHelper.OnEntry(this, action);
        }

        public StateConfiguration<TState, TTrigger> OnExit(Action<Transition<TState, TTrigger>> action)
        {
            return StateConfigurationMethodHelper.OnExit(this, action);
        }

        public StateConfiguration<TState, TTrigger> OnExit(Action action)
        {
            return StateConfigurationMethodHelper.OnExit(this, t => action());
        }

        #endregion

        #region Permit

        public StateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, resultingState, null);
        }

        public StateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, resultingState, t => onEntryAction());
        }

        public StateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, resultingState, onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, resultingState, (t, a) => onEntryAction
                (a));
        }

        public StateConfiguration<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, resultingState, onEntryAction);
        }

        #endregion

        #region PermitIf

        public StateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, resultingState, null);
        }

        public StateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, resultingState, t => onEntryAction());
        }

        public StateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, resultingState, onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, resultingState,
                (t, a) => onEntryAction(a));
        }

        public StateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, resultingState, onEntryAction);
        }

        #endregion

        #region PermitReentry

        public StateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, CurrentStateRepresentation.State, null);
        }

        public StateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, CurrentStateRepresentation.State,
                t => onEntryAction());
        }

        public StateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, CurrentStateRepresentation.State,
                onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, CurrentStateRepresentation.State,
                (t, a) => onEntryAction(a));
        }

        public StateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, null, trigger, CurrentStateRepresentation.State,
                onEntryAction);
        }

        #endregion

        #region PermitReentryIf

        public StateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, CurrentStateRepresentation.State,
                null);
        }

        public StateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, CurrentStateRepresentation.State,
                t => onEntryAction());
        }

        public StateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, CurrentStateRepresentation.State,
                onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, CurrentStateRepresentation.State,
                (t, a) => onEntryAction(a));
        }

        public StateConfiguration<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(CurrentStateRepresentation.State != null);

            return StateConfigurationMethodHelper.Permit(this, predicate, trigger, CurrentStateRepresentation.State,
                onEntryAction);
        }

        #endregion

        #region Ignore and IgnoreIf

        public StateConfiguration<TState, TTrigger> Ignore(TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return StateConfigurationMethodHelper.Ignore(this, null, trigger);
        }

        public StateConfiguration<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return StateConfigurationMethodHelper.Ignore(this, predicate, trigger);
        }

        #endregion

        #region Dynamic

        public StateConfiguration<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStatePredicate != null);

            return StateConfigurationMethodHelper.PermitDynamic(this, trigger, targetStatePredicate,
                t => onEntryAction());
        }

        public StateConfiguration<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStatePredicate != null);

            return StateConfigurationMethodHelper.PermitDynamic(this, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStatePredicate != null);

            return StateConfigurationMethodHelper.PermitDynamic(this, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(targetStatePredicate != null);

            return StateConfigurationMethodHelper.PermitDynamic(this, trigger, targetStatePredicate,
                (t, a) => onEntryAction(a));
        }

        #endregion
    }
}
