// Author: Prasanna V. Loganathar
// Created: 1:33 AM 05-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LiquidState.Common;

namespace LiquidState.Machines
{
    [ContractClass(typeof (StateMachineContract<,>))]
    public interface IStateMachine<TState, TTrigger>
    {
        IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        TState CurrentState { get; }
        bool IsEnabled { get; }
        bool IsInTransition { get; }
        event Action<TState, TState> StateChanged;
        event Action<TTrigger, TState> UnhandledTriggerExecuted;
        bool CanHandleTrigger(TTrigger trigger);
        bool CanTransitionTo(TState state);
        void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument);
        void Fire(TTrigger trigger);
        void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default);
        void Pause();
        void Resume();
    }

    [ContractClassFor(typeof (IStateMachine<,>))]
    public abstract class StateMachineContract<T, U> : IStateMachine<T, U>
    {
        public abstract event Action<U, T> UnhandledTriggerExecuted;
        public abstract event Action<T, T> StateChanged;
        public abstract bool CanHandleTrigger(U trigger);
        public abstract bool CanTransitionTo(T state);
        public abstract void MoveToState(T state, StateTransitionOption option = StateTransitionOption.Default);
        public abstract void Pause();
        public abstract void Resume();

        public void Fire<TArgument>(ParameterizedTrigger<U, TArgument> parameterizedTrigger, TArgument argument)
        {
            Contract.Requires<NullReferenceException>(parameterizedTrigger != null);
        }

        public abstract void Fire(U trigger);
        public abstract T CurrentState { get; }
        public abstract IEnumerable<U> CurrentPermittedTriggers { get; }
        public abstract bool IsEnabled { get; }
        public abstract bool IsInTransition { get; }
    }
}
