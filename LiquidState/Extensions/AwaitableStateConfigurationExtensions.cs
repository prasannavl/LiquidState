// Author: Prasanna V. Loganathar
// Created: 22:55 17-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Awaitable.Core;
using LiquidState.Core;

namespace LiquidState
{
    public static class AwaitableStateConfigurationExtensions
    {
        #region Entry & Exit

        public static AwaitableStateConfiguration<TState, TTrigger> OnExit<TState, TTrigger>(this
            AwaitableStateConfiguration<TState, TTrigger> config, Action action)
        {
            Contract.Requires<ArgumentNullException>(action != null, nameof(action));

            return config.OnExit(t => action());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> OnExit<TState, TTrigger>(this
            AwaitableStateConfiguration<TState, TTrigger> config, Func<Task> action)
        {
            Contract.Requires<ArgumentNullException>(action != null, nameof(action));

            return config.OnExit(t => action());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> OnEntry<TState, TTrigger>(this
            AwaitableStateConfiguration<TState, TTrigger> config, Action action)
        {
            Contract.Requires<ArgumentNullException>(action != null, nameof(action));

            return config.OnEntry(t => action());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> OnEntry<TState, TTrigger>(this
            AwaitableStateConfiguration<TState, TTrigger> config, Func<Task> action)
        {
            Contract.Requires<ArgumentNullException>(action != null, nameof(action));

            return config.OnEntry(t => action());
        }

        #endregion

        #region Permit

        public static AwaitableStateConfiguration<TState, TTrigger> Permit<TState, TTrigger>(this
            AwaitableStateConfiguration<TState, TTrigger> config, TTrigger trigger,
            TState resultingState,
            Action onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.Permit(trigger, resultingState, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> Permit<TState, TTrigger>(this
            AwaitableStateConfiguration<TState, TTrigger> config, TTrigger trigger,
            TState resultingState,
            Func<Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.Permit(trigger, resultingState, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> Permit<TState, TTrigger, TArgument>(this
            AwaitableStateConfiguration<TState, TTrigger> config,
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<TArgument> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.Permit(trigger, resultingState, (t, a) => onTriggerAction(a));
        }

        public static AwaitableStateConfiguration<TState, TTrigger> Permit<TState, TTrigger, TArgument>(this
            AwaitableStateConfiguration<TState, TTrigger> config,
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Func<TArgument, Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.Permit(trigger, resultingState, (t, a) => onTriggerAction(a));
        }

        #endregion

        #region PermitReentry

        public static AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TState, TTrigger>(this
            AwaitableStateConfiguration<TState, TTrigger> config, TTrigger trigger,
            Action onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentry(trigger, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitReentry<TState, TTrigger>(this
            AwaitableStateConfiguration<TState, TTrigger> config, TTrigger trigger,
            Func<Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentry(trigger, t => onTriggerAction());
        }

        #endregion

        #region PermitReentryIf

        public static AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<bool> predicate,
            TTrigger trigger,
            Action onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentryIf(predicate, trigger, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentryIf(predicate, trigger, (t, a) => onTriggerAction(a));
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<Task<bool>> predicate,
            TTrigger trigger, Action onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentryIf(predicate, trigger, (t) => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitReentryIf<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitReentryIf(predicate, trigger, (t, a) => onTriggerAction(a));
        }

        #endregion

        #region PermitIf

        public static AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState, (t, a) => onTriggerAction(a));
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState, (t, a) => onTriggerAction(a));
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState, (t, a) => onTriggerAction(a));
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitIf<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<bool> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitIf<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitIf<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState, (t, a) => onTriggerAction(a));
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitIf<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitIf<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Action onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitIf(predicate, trigger, resultingState, t => onTriggerAction());
        }

        #endregion

        #region Dynamic

        public static AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, TTrigger trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Action onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStateFunc, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, TTrigger trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStateFunc, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStateFunc, t => onTriggerAction());
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, TTrigger trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStateFunc, t => onTriggerAction());
        }


        public static AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Func<TArgument, Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStateFunc, (t, a) => onTriggerAction(a));
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Action<TArgument> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStateFunc, (t, a) => onTriggerAction(a));
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<DynamicState<TState>>> targetStateFunc,
            Action<TArgument> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStateFunc, (t, a) => onTriggerAction(a));
        }

        public static AwaitableStateConfiguration<TState, TTrigger> PermitDynamic<TArgument, TState, TTrigger>(
            this AwaitableStateConfiguration<TState, TTrigger> config, ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<DynamicState<TState>> targetStateFunc,
            Func<TArgument, Task> onTriggerAction)
        {
            Contract.Requires<ArgumentNullException>(onTriggerAction != null, nameof(onTriggerAction));

            return config.PermitDynamic(trigger, targetStateFunc, (t, a) => onTriggerAction(a));
        }

        #endregion
    }
}