// Author: Prasanna V. Loganathar
// Created: 22:55 17-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using LiquidState.Core;
using LiquidState.Synchronous.Core;
using LiquidState.Common;

namespace LiquidState
{
    public static class StateConfigurationExtensions
    {
        public static StateConfiguration<TState, TTrigger> OnEntry<TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config, Action action)
        {
            Contract.NotNull(action != null, nameof(action));

            return config.OnEntry(t => action());
        }

        public static StateConfiguration<TState, TTrigger> OnExit<TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config, Action action)
        {
            Contract.NotNull(action != null, nameof(action));

            return config.OnExit(t => action());
        }

        public static StateConfiguration<TState, TTrigger> Permit<TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config, TTrigger trigger, TState resultingState,
            Action onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.Permit(trigger, resultingState,
                t => onTriggerAction());
        }

        public static StateConfiguration<TState, TTrigger> Permit<TState, TTrigger, TArgument>(this
                StateConfiguration<TState, TTrigger> config, ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState,
            Action<TArgument> onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.Permit(trigger, resultingState,
                (t, a) => onTriggerAction
                    (a));
        }

        public static StateConfiguration<TState, TTrigger> PermitIf<TState, TTrigger>(
            StateConfiguration<TState, TTrigger> config, Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState,
                t => onTriggerAction());
        }

        public static StateConfiguration<TState, TTrigger> PermitIf<TArgument, TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config, Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState,
                (t, a) => onTriggerAction(a));
        }

        public static StateConfiguration<TState, TTrigger> PermitReentry<TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config, TTrigger trigger, Action onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentry(trigger,
                t => onTriggerAction());
        }

        public static StateConfiguration<TState, TTrigger> PermitReentry<TArgument, TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config,
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentry(trigger,
                (t, a) => onTriggerAction(a));
        }

        public static StateConfiguration<TState, TTrigger> PermitReentryIf<TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config, Func<bool> predicate, TTrigger trigger,
            Action onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentryIf(predicate, trigger,
                t => onTriggerAction());
        }

        public static StateConfiguration<TState, TTrigger> PermitReentryIf<TArgument, TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config, Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentryIf(predicate, trigger,
                (t, a) => onTriggerAction(a));
        }

        public static StateConfiguration<TState, TTrigger> PermitDynamic<TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config, TTrigger trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStatePredicate,
                t => onTriggerAction());
        }

        public static StateConfiguration<TState, TTrigger> PermitDynamic<TArgument, TState, TTrigger>(
            this StateConfiguration<TState, TTrigger> config,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStatePredicate,
            Action<TArgument> onTriggerAction)
        {
            Contract.NotNull(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStatePredicate,
                (t, a) => onTriggerAction(a));
        }
    }
}