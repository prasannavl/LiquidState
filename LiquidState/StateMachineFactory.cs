// Author: Prasanna V. Loganathar
// Created: 2:11 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
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
            Configuration<TState, TTrigger> config, bool blocking = false, bool throwOnInvalidTriggers = true)
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
            return sm;
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> config, bool asyncMachine = true, bool throwOnInvalidTriggers = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            return Create(initialState, config, asyncMachine, null, throwOnInvalidTriggers);
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> config, TaskScheduler customScheduler, bool throwOnInvalidTriggers = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);
            Contract.Requires<ArgumentNullException>(customScheduler != null);

            return Create(initialState, config, false, null, throwOnInvalidTriggers);
        }

        public static Configuration<TState, TTrigger> CreateConfiguration<TState, TTrigger>()
        {
            return new Configuration<TState, TTrigger>();
        }

        public static AwaitableStateMachineConfiguration<TState, TTrigger> CreateAwaitableConfiguration
            <TState, TTrigger>()
        {
            return new AwaitableStateMachineConfiguration<TState, TTrigger>();
        }

        private static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> config, bool asyncMachine, TaskScheduler scheduler, bool throwOnInvalidTriggers)
        {
            IAwaitableStateMachine<TState, TTrigger> sm;
            if (asyncMachine)
            {
                sm = new AsyncStateMachine<TState, TTrigger>(initialState, config);
            }
            else
            {
                sm = scheduler == null
                    ? (IAwaitableStateMachine<TState, TTrigger>)
                        new AwaitableStateMachine<TState, TTrigger>(initialState, config)
                    : new AwaitableStateMachineWithScheduler<TState, TTrigger>(initialState, config, scheduler);
            }

            if (throwOnInvalidTriggers)
                sm.UnhandledTriggerExecuted += InvalidTriggerException<TTrigger, TState>.Throw;
            return sm;
        }
    }
}
