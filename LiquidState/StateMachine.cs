// Author: Prasanna V. Loganathar
// Created: 2:11 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.Contracts;
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

            return new StateMachine<TState, TTrigger>(initialState, config);
        }

        public static AsyncStateMachine<TState, TTrigger> Create<TState, TTrigger>(TState initialState,
            AsyncStateMachineConfiguration<TState, TTrigger> config)
        {
            Contract.Requires<ArgumentNullException>(initialState != null);
            Contract.Requires<ArgumentNullException>(config != null);

            return new AsyncStateMachine<TState, TTrigger>(initialState, config);
        }

        public static StateMachineConfiguration<TState, TTrigger> CreateConfiguration<TState, TTrigger>()
        {
            return new StateMachineConfiguration<TState, TTrigger>();
        }

        public static AsyncStateMachineConfiguration<TState, TTrigger> CreateAsyncConfiguration<TState, TTrigger>()
        {
            return new AsyncStateMachineConfiguration<TState, TTrigger>();
        }

        public static StateMachineConfiguration<TState, TTrigger> Reconfigure<TState, TTrigger>(
            StateMachine<TState, TTrigger> existingMachine)
        {
            Contract.Requires<ArgumentNullException>(existingMachine != null);

            return new StateMachineConfiguration<TState, TTrigger>(existingMachine);
        }

        public static AsyncStateMachineConfiguration<TState, TTrigger> Reconfigure<TState, TTrigger>(
            AsyncStateMachine<TState, TTrigger> existingAsyncMachine)
        {
            Contract.Requires<ArgumentNullException>(existingAsyncMachine != null);

            return new AsyncStateMachineConfiguration<TState, TTrigger>(existingAsyncMachine);
        }
    }
}