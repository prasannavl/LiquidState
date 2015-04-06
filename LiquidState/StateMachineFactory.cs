// Author: Prasanna V. Loganathar
// Created: 2:11 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LiquidState.Common;
using LiquidState.Configuration;
using LiquidState.Machines;

namespace LiquidState
{
    public static class StateMachineFactory
    {
        public static IStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            StateMachineConfiguration<TState, TTrigger> config, bool blocking = false)
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
                sm = new StateMachine<TState, TTrigger>(initialState, config);
            }

            sm.UnhandledTriggerExecuted += InvalidTriggerException<TTrigger, TState>.Throw;
            return sm;
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> config, bool asyncMachine = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            return Create(initialState, config, asyncMachine, null);
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(
            TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> config, TaskScheduler customScheduler)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);
            Contract.Requires<ArgumentNullException>(customScheduler != null);

            return Create(initialState, config, false, null);
        }

        public static StateMachineConfiguration<TState, TTrigger> CreateConfiguration<TState, TTrigger>()
        {
            return new StateMachineConfiguration<TState, TTrigger>();
        }

        public static AwaitableStateMachineConfiguration<TState, TTrigger> CreateAwaitableConfiguration
            <TState, TTrigger>()
        {
            return new AwaitableStateMachineConfiguration<TState, TTrigger>();
        }

        private static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> config, bool asyncMachine, TaskScheduler scheduler)
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

            sm.UnhandledTriggerExecuted += InvalidTriggerException<TTrigger, TState>.Throw;
            return sm;
        }
    }
}
