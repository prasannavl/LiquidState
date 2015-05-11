// Author: Prasanna V. Loganathar
// Created: 1:33 AM 05-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Synchronous.Core
{
    [ContractClass(typeof (StateMachineContract<,>))]
    public interface IStateMachine<TState, TTrigger> : IStateMachineCore<TState, TTrigger>
    {
        void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument);
        void Fire(TTrigger trigger);
        void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default);
    }

    [ContractClassFor(typeof (IStateMachine<,>))]
    public abstract class StateMachineContract<TState, TTrigger> : IStateMachine<TState, TTrigger>
    {
        public abstract void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default);
        public abstract void Pause();
        public abstract void Resume();

        public abstract event Action<TransitionExecutedEventArgs<TState, TTrigger>> TransitionExecuted;

        public void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument)
        {
            Contract.Requires<NullReferenceException>(parameterizedTrigger != null);
        }

        public abstract void Fire(TTrigger trigger);
        public abstract TState CurrentState { get; }
        public abstract IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        public abstract bool IsEnabled { get; }

        public abstract event Action<TriggerStateEventArgs<TState, TTrigger>> UnhandledTrigger;
        public abstract event Action<TransitionEventArgs<TState, TTrigger>> InvalidState;
        public abstract event Action<TransitionEventArgs<TState, TTrigger>> TransitionStarted;
    }
}
