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
            Synchronous.Core.Configuration<TState, TTrigger> config, bool blocking = false,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
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

            if (throwOnInvalidTriggers)
                sm.UnhandledTrigger += InvalidTriggerException<TTrigger, TState>.Throw;

            if (throwOnInvalidState)
                sm.InvalidState += InvalidStateException<TState>.Throw;

            return sm;
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState, AwaitableConfiguration<TState, TTrigger> config, bool queued = true,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            return Create(initialState, config, queued, null, throwOnInvalidTriggers, throwOnInvalidState);
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState, AwaitableConfiguration<TState, TTrigger> config, TaskScheduler customScheduler,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);
            Contract.Requires<ArgumentNullException>(customScheduler != null);

            return Create(initialState, config, false, null, throwOnInvalidTriggers, throwOnInvalidState);
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

        private static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            AwaitableConfiguration<TState, TTrigger> config, bool queued, TaskScheduler scheduler,
            bool throwOnInvalidTriggers, bool throwOnInvalidState = true)
        {
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

            if (throwOnInvalidTriggers)
                sm.UnhandledTrigger += InvalidTriggerException<TTrigger, TState>.Throw;

            if (throwOnInvalidState)
                sm.InvalidState += InvalidStateException<TState>.Throw;

            return sm;
        }
    }
}
