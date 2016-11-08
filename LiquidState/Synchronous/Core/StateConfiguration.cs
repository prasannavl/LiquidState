// Author: Prasanna V. Loganathar
// Created: 09:55 16-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using LiquidState.Core;
using Helper = LiquidState.Synchronous.Core.StateConfigurationMethodHelper;

namespace LiquidState.Synchronous.Core
{
    public class StateConfiguration<TState, TTrigger>
    {
        internal readonly StateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        internal readonly Dictionary<TState, StateRepresentation<TState, TTrigger>> Representations;

        internal StateConfiguration(Dictionary<TState, StateRepresentation<TState, TTrigger>> representations,
            TState currentState)
        {
            Representations = representations;
            CurrentStateRepresentation = StateConfigurationHelper.FindOrCreateStateRepresentation(currentState,
                representations);
        }

        #region Entry and Exit

        public StateConfiguration<TState, TTrigger> OnEntry(Action<Transition<TState, TTrigger>> action)
        {
            return Helper.OnEntry(this, action);
        }

        public StateConfiguration<TState, TTrigger> OnExit(Action<Transition<TState, TTrigger>> action)
        {
            return Helper.OnExit(this, action);
        }

        #endregion

        #region Permit

        public StateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            return Helper.Permit(this, null, trigger, resultingState, null);
        }


        public StateConfiguration<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            return Helper.Permit(this, null, trigger, resultingState, onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            return Helper.Permit(this, null, trigger, resultingState, onEntryAction);
        }

        #endregion

        #region PermitIf

        public StateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            return Helper.Permit(this, predicate, trigger, resultingState, null);
        }

        public StateConfiguration<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action<Transition<TState, TTrigger>> onEntryAction)
        {
            return Helper.Permit(this, predicate, trigger, resultingState, onEntryAction);
        }


        public StateConfiguration<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            return Helper.Permit(this, predicate, trigger, resultingState, onEntryAction);
        }

        #endregion

        #region PermitReentry

        public StateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            return Helper.Permit(this, null, trigger, CurrentStateRepresentation.State, null);
        }

        public StateConfiguration<TState, TTrigger> PermitReentry(TTrigger trigger,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            return Helper.Permit(this, null, trigger, CurrentStateRepresentation.State,
                onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            return Helper.Permit(this, null, trigger, CurrentStateRepresentation.State,
                onEntryAction);
        }

        #endregion

        #region PermitReentryIf

        public StateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger)
        {
            return Helper.Permit(this, predicate, trigger, CurrentStateRepresentation.State,
                null);
        }


        public StateConfiguration<TState, TTrigger> PermitReentryIf(Func<bool> predicate, TTrigger trigger,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            return Helper.Permit(this, predicate, trigger, CurrentStateRepresentation.State,
                onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            return Helper.Permit(this, predicate, trigger, CurrentStateRepresentation.State,
                onEntryAction);
        }

        #endregion

        #region Ignore and IgnoreIf

        public StateConfiguration<TState, TTrigger> Ignore(TTrigger trigger)
        {
            return Helper.Ignore(this, null, trigger);
        }

        public StateConfiguration<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            return Helper.Ignore(this, predicate, trigger);
        }

        #endregion

        #region Dynamic

        public StateConfiguration<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<Transition<TState, TTrigger>> onEntryAction)
        {
            return Helper.PermitDynamic(this, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfiguration<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<Transition<TState, TTrigger>, TArgument> onEntryAction)
        {
            return Helper.PermitDynamic(this, trigger, targetStatePredicate, onEntryAction);
        }

        #endregion
    }
}