// Author: Prasanna V. Loganathar
// Created: 2:11 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.Contracts;
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

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> config, bool asyncMachine = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            IAwaitableStateMachine<TState, TTrigger> sm;
            if (asyncMachine)
            {
                sm = new AsyncStateMachine<TState, TTrigger>(initialState, config);
            }
            else
            {
                sm = new AwaitableStateMachine<TState, TTrigger>(initialState, config);
            }

            sm.UnhandledTriggerExecuted += InvalidTriggerException<TTrigger, TState>.Throw;
            return sm;
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
    }
}
