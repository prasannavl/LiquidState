// Author: Prasanna V. Loganathar
// Created: 11:43 16-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.Contracts;
using LiquidState.Core;
using LiquidState.Common;

namespace LiquidState.Synchronous.Core
{
    internal static class StateConfigurationMethodHelper
    {
        internal static StateConfiguration<TState, TTrigger> OnEntry<TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, Action<Transition<TState, TTrigger>> action)
        {
            config.CurrentStateRepresentation.OnEntryAction = action;
            return config;
        }

        internal static StateConfiguration<TState, TTrigger> OnExit<TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, Action<Transition<TState, TTrigger>> action)
        {
            config.CurrentStateRepresentation.OnExitAction = action;
            return config;
        }

        internal static StateConfiguration<TState, TTrigger> Permit<TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return PermitCore(config, predicate, trigger, resultingState, onTriggerAction);
        }

        internal static StateConfiguration<TState, TTrigger> Permit<TArgument, TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            return PermitCore(config, predicate, trigger.Trigger, resultingState, onTriggerAction);
        }

        internal static StateConfiguration<TState, TTrigger> PermitDynamic<TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate, Action<Transition<TState, TTrigger>> onTriggerAction)
        {
            return PermitDynamicCore(config, trigger, targetStatePredicate, onTriggerAction);
        }

        internal static StateConfiguration<TState, TTrigger> PermitDynamic<TArgument, TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<Transition<TState, TTrigger>, TArgument> onTriggerAction)
        {
            return PermitDynamicCore(config, trigger.Trigger, targetStatePredicate, onTriggerAction);
        }

        internal static StateConfiguration<TState, TTrigger> Ignore<TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, Func<bool> predicate, TTrigger trigger)
        {
            Contract.NotNull(trigger != null, nameof(trigger));

            if (
                StateConfigurationHelper.FindTriggerRepresentation(trigger,
                    config.CurrentStateRepresentation) != null) ExceptionHelper.ThrowExclusiveOperation();

            var rep = StateConfigurationHelper.CreateTriggerRepresentation(trigger,
                config.CurrentStateRepresentation);
            rep.NextStateRepresentationWrapper = null;
            rep.ConditionalTriggerPredicate = predicate;

            return config;
        }

        private static StateConfiguration<TState, TTrigger> PermitCore<TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, Func<bool> predicate, TTrigger trigger,
            TState resultingState, object onTriggerAction)
        {
            Contract.NotNull(trigger != null, nameof(trigger));
            Contract.NotNull(resultingState != null, nameof(resultingState));

            if (StateConfigurationHelper.FindTriggerRepresentation(trigger, config.CurrentStateRepresentation) !=
                null) ExceptionHelper.ThrowExclusiveOperation();

            var rep = StateConfigurationHelper.CreateTriggerRepresentation(trigger,
                config.CurrentStateRepresentation);
            rep.NextStateRepresentationWrapper = StateConfigurationHelper.FindOrCreateStateRepresentation(
                resultingState, config.Representations);
            rep.OnTriggerAction = onTriggerAction;
            rep.ConditionalTriggerPredicate = predicate;

            return config;
        }

        private static StateConfiguration<TState, TTrigger> PermitDynamicCore<TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate, object onTriggerAction)
        {
            Contract.NotNull(trigger != null, nameof(trigger));
            Contract.NotNull(targetStatePredicate != null, nameof(targetStatePredicate));

            if (
                StateConfigurationHelper.FindTriggerRepresentation(trigger,
                    config.CurrentStateRepresentation) != null) ExceptionHelper.ThrowExclusiveOperation();

            var rep = StateConfigurationHelper.CreateTriggerRepresentation(trigger,
                config.CurrentStateRepresentation);
            rep.NextStateRepresentationWrapper = targetStatePredicate;
            rep.OnTriggerAction = onTriggerAction;
            rep.ConditionalTriggerPredicate = null;
            rep.TransitionFlags |= TransitionFlag.DynamicState;

            return config;
        }
    }
}