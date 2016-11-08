// Author: Prasanna V. Loganathar
// Created: 02:11 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Threading.Tasks;
using LiquidState.Awaitable;
using LiquidState.Awaitable.Core;
using LiquidState.Core;
using LiquidState.Synchronous;
using LiquidState.Synchronous.Core;
using LiquidState.Common;

namespace LiquidState
{
    public static class StateMachineFactory
    {
        public static IStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            Configuration<TState, TTrigger> config, bool blocking = false,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.NotNull(config != null, nameof(config));

            return CreateCore<TState, TTrigger, object>(null, _ => CreateDefault(initialState, config, blocking),
                throwOnInvalidTriggers,
                throwOnInvalidState);
        }

        public static IStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            Func<IStateMachine<TState, TTrigger>> stateMachineFunc,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.NotNull(stateMachineFunc != null, nameof(stateMachineFunc));

            return CreateCore<TState, TTrigger, object>(null, _ => stateMachineFunc(), throwOnInvalidTriggers,
                throwOnInvalidState);
        }

        public static IStateMachine<TState, TTrigger> Create<TState, TTrigger, TOptions>(TOptions options,
            Func<TOptions, IStateMachine<TState, TTrigger>> stateMachineFunc,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.NotNull(stateMachineFunc != null, nameof(stateMachineFunc));

            return CreateCore(options, stateMachineFunc, throwOnInvalidTriggers, throwOnInvalidState);
        }


        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState, AwaitableConfiguration<TState, TTrigger> config, bool queued = true,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.NotNull(config != null, nameof(config));

            return CreateCore<TState, TTrigger, object>(null, _ => CreateDefault(initialState, config, queued, null),
                throwOnInvalidTriggers,
                throwOnInvalidState);
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState, AwaitableConfiguration<TState, TTrigger> config, TaskScheduler scheduler,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.NotNull(config != null, nameof(config));
            Contract.NotNull(scheduler != null, nameof(scheduler));

            return CreateCore<TState, TTrigger, object>(null, _ => CreateDefault(initialState, config, false, scheduler),
                throwOnInvalidTriggers,
                throwOnInvalidState);
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            Func<IAwaitableStateMachine<TState, TTrigger>> stateMachineFunc,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.NotNull(stateMachineFunc != null, nameof(stateMachineFunc));

            return CreateCore<TState, TTrigger, object>(null, _ => stateMachineFunc(), throwOnInvalidTriggers,
                throwOnInvalidState);
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger, TOptions>(TOptions options,
            Func<TOptions, IAwaitableStateMachine<TState, TTrigger>> stateMachineFunc,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.NotNull(stateMachineFunc != null, nameof(stateMachineFunc));

            return CreateCore(options, stateMachineFunc, throwOnInvalidTriggers, throwOnInvalidState);
        }

        #region Core Methods

        public static Configuration<TState, TTrigger> CreateConfiguration<TState, TTrigger>()
        {
            return new Configuration<TState, TTrigger>();
        }

        public static AwaitableConfiguration<TState, TTrigger> CreateAwaitableConfiguration
            <TState, TTrigger>()
        {
            return new AwaitableConfiguration<TState, TTrigger>();
        }

        private static IStateMachine<TState, TTrigger> CreateCore<TState, TTrigger, TOptions>(TOptions options,
            Func<TOptions, IStateMachine<TState, TTrigger>> stateMachineFunc,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            var sm = stateMachineFunc(options);
            if (sm == null) throw new InvalidOperationException("State machine must be initializable");

            Configure(sm, throwOnInvalidTriggers, throwOnInvalidState);
            return sm;
        }

        private static IAwaitableStateMachine<TState, TTrigger> CreateCore<TState, TTrigger, TOptions>(TOptions options,
            Func<TOptions, IAwaitableStateMachine<TState, TTrigger>> stateMachineFunc,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            var sm = stateMachineFunc(options);
            if (sm == null) throw new InvalidOperationException("State machine must be initializable");

            Configure(sm, throwOnInvalidTriggers, throwOnInvalidState);
            return sm;
        }

        #endregion

        #region Default Helpers

        private static IStateMachine<TState, TTrigger> CreateDefault<TState, TTrigger>(TState initialState,
            Configuration<TState, TTrigger> config, bool blocking = false)
        {
            IStateMachine<TState, TTrigger> sm;
            if (blocking) { sm = new BlockingStateMachine<TState, TTrigger>(initialState, config); }
            else
            { sm = new GuardedStateMachine<TState, TTrigger>(initialState, config); }

            return sm;
        }

        private static IAwaitableStateMachine<TState, TTrigger> CreateDefault<TState, TTrigger>(
            TState initialState,
            AwaitableConfiguration<TState, TTrigger> config, bool queued, TaskScheduler scheduler)
        {
            IAwaitableStateMachine<TState, TTrigger> sm;
            if (queued) { sm = new QueuedAwaitableStateMachine<TState, TTrigger>(initialState, config); }
            else
            {
                sm = scheduler == null
                    ? (IAwaitableStateMachine<TState, TTrigger>)
                    new GuardedAwaitableStateMachine<TState, TTrigger>(initialState, config)
                    : new ScheduledAwaitableStateMachine<TState, TTrigger>(initialState, config, scheduler);
            }

            return sm;
        }

        private static void Configure<TState, TTrigger>(IStateMachineCore<TState, TTrigger> stateMachine,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            if (throwOnInvalidTriggers) stateMachine.UnhandledTrigger += ExceptionHelper.ThrowInvalidTrigger;
            if (throwOnInvalidState) stateMachine.InvalidState += ExceptionHelper.ThrowInvalidState;
        }

        #endregion
    }
}