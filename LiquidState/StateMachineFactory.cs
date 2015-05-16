// Author: Prasanna V. Loganathar
// Created: 2:11 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Awaitable;
using LiquidState.Core;
using LiquidState.Synchronous;

namespace LiquidState
{
    public static class StateMachineFactory
    {
        public static Synchronous.Core.IStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            Synchronous.Core.Configuration<TState, TTrigger> config, bool blocking = false,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            Synchronous.Core.IStateMachine<TState, TTrigger> sm;
            if (blocking)
            {
                sm = new BlockingStateMachine<TState, TTrigger>(initialState, config);
            }
            else
            {
                sm = new Synchronous.GuardedStateMachine<TState, TTrigger>(initialState, config);
            }

            if (throwOnInvalidTriggers)
                sm.UnhandledTrigger += InvalidTriggerException<TTrigger, TState>.Throw;

            if (throwOnInvalidState)
                sm.InvalidState += InvalidStateException<TState>.Throw;

            return sm;
        }

        public static Awaitable.Core.IStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState, Awaitable.Core.Configuration<TState, TTrigger> config, bool queued = true,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            return Create(initialState, config, queued, null, throwOnInvalidTriggers, throwOnInvalidState);
        }

        public static Awaitable.Core.IStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState, Awaitable.Core.Configuration<TState, TTrigger> config, TaskScheduler customScheduler,
            bool throwOnInvalidTriggers = true, bool throwOnInvalidState = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);
            Contract.Requires<ArgumentNullException>(customScheduler != null);

            return Create(initialState, config, false, null, throwOnInvalidTriggers, throwOnInvalidState);
        }

        public static Synchronous.Core.Configuration<TState, TTrigger> CreateConfiguration<TState, TTrigger>()
        {
            return new Synchronous.Core.Configuration<TState, TTrigger>();
        }

        public static Awaitable.Core.Configuration<TState, TTrigger> CreateAwaitableConfiguration
            <TState, TTrigger>()
        {
            return new Awaitable.Core.Configuration<TState, TTrigger>();
        }

        private static Awaitable.Core.IStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            Awaitable.Core.Configuration<TState, TTrigger> config, bool queued, TaskScheduler scheduler,
            bool throwOnInvalidTriggers, bool throwOnInvalidState = true)
        {
            Awaitable.Core.IStateMachine<TState, TTrigger> sm;
            if (queued)
            {
                sm = new QueuedStateMachine<TState, TTrigger>(initialState, config);
            }
            else
            {
                sm = scheduler == null
                    ? (Awaitable.Core.IStateMachine<TState, TTrigger>)
                        new Awaitable.GuardedStateMachine<TState, TTrigger>(initialState, config)
                    : new ScheduledStateMachine<TState, TTrigger>(initialState, config, scheduler);
            }

            if (throwOnInvalidTriggers)
                sm.UnhandledTrigger += InvalidTriggerException<TTrigger, TState>.Throw;

            if (throwOnInvalidState)
                sm.InvalidState += InvalidStateException<TState>.Throw;

            return sm;
        }
    }
}
