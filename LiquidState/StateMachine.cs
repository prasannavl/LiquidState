﻿// Author: Prasanna V. Loganathar
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
    public static class StateMachine
    {
        public static StateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            StateMachineConfiguration<TState, TTrigger> config)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            var sm = new StateMachine<TState, TTrigger>(initialState, config);
            sm.UnhandledTriggerExecuted += InvalidTriggerException<TTrigger, TState>.Throw;

            return sm;
        }

        public static IAwaitableStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> config, bool asyncMachine = true)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            if (asyncMachine)
            {
                var sm = new AsyncStateMachine<TState, TTrigger>(initialState, config);
                sm.UnhandledTriggerExecuted += InvalidTriggerException<TTrigger, TState>.Throw;
                return sm;
            }
            else
            {
                var sm = new AwaitableStateMachine<TState, TTrigger>(initialState, config);
                sm.UnhandledTriggerExecuted += InvalidTriggerException<TTrigger, TState>.Throw;
                return sm;
            }
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
