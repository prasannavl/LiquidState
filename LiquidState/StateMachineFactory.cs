// Author: Prasanna V. Loganathar
// Created: 2:11 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Awaitable;
using LiquidState.Awaitable.Core;
using LiquidState.Core;
using LiquidState.Synchronous;
using LiquidState.Synchronous.Core;

namespace LiquidState
{
    public static class StateMachineFactory
    {
        public static IStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            Configuration<TState, TTrigger> config, bool blocking = false,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            return Create(() => CreateDefault(initialState, config, blocking), throwOnInvalidTriggers,
                throwOnInvalidState);
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState, AwaitableConfiguration<TState, TTrigger> config, bool queued = true,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            return Create(() => CreateDefault(initialState, config, queued, null), throwOnInvalidTriggers,
                throwOnInvalidState);
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState, AwaitableConfiguration<TState, TTrigger> config, TaskScheduler scheduler,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.Requires<ArgumentNullException>(scheduler != null);

            return Create(() => CreateDefault(initialState, config, false, scheduler), throwOnInvalidTriggers,
                throwOnInvalidState);
        }

        public static Configuration<TState, TTrigger> CreateConfiguration<TState, TTrigger>()
        {
            return new Configuration<TState, TTrigger>();
        }

        public static AwaitableConfiguration<TState, TTrigger> CreateAwaitableConfiguration
            <TState, TTrigger>()
        {
            return new AwaitableConfiguration<TState, TTrigger>();
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            Func<IAwaitableStateMachine<TState, TTrigger>> stateMachineFunc,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            var sm = stateMachineFunc();
            Configure(sm, throwOnInvalidTriggers, throwOnInvalidState);
            return sm;
        }

        public static IStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            Func<IStateMachine<TState, TTrigger>> stateMachineFunc,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            var sm = stateMachineFunc();
            Configure(sm, throwOnInvalidTriggers, throwOnInvalidState);
            return sm;
        }

        private static void Configure<TState, TTrigger>(IStateMachineCore<TState, TTrigger> stateMachine,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            if (throwOnInvalidTriggers)
                stateMachine.UnhandledTrigger += InvalidTriggerException<TTrigger, TState>.Throw;

            if (throwOnInvalidState)
                stateMachine.InvalidState += InvalidStateException<TState>.Throw;
        }

        private static IStateMachine<TState, TTrigger> CreateDefault<TState, TTrigger>(TState initialState,
            Configuration<TState, TTrigger> config, bool blocking = false)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            IStateMachine<TState, TTrigger> sm;
            if (blocking)
            {
                sm = new BlockingStateMachine<TState, TTrigger>(initialState, config);
            }
            else
            {
                sm = new GuardedStateMachine<TState, TTrigger>(initialState, config);
            }

            return sm;
        }

        private static IAwaitableStateMachine<TState, TTrigger> CreateDefault<TState, TTrigger>(
            TState initialState,
            AwaitableConfiguration<TState, TTrigger> config, bool queued, TaskScheduler scheduler)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            IAwaitableStateMachine<TState, TTrigger> sm;
            if (queued)
            {
                sm = new QueuedAwaitableStateMachine<TState, TTrigger>(initialState, config);
            }
            else
            {
                sm = scheduler == null
                    ? (IAwaitableStateMachine<TState, TTrigger>)
                        new GuardedAwaitableStateMachine<TState, TTrigger>(initialState, config)
                    : new ScheduledAwaitableStateMachine<TState, TTrigger>(initialState, config, scheduler);
            }

            return sm;
        }
    }
}
