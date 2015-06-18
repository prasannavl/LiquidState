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
        IStateMachineDiagnostics<TState, TTrigger> Diagnostics { get; }
        void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument);
        void Fire(TTrigger trigger);
        void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default);
    }

    [ContractClass(typeof (StateMachineDiagnosticsContract<,>))]
    public interface IStateMachineDiagnostics<TState, TTrigger> : IStateMachineDiagnosticsCore<TState, TTrigger>
    {
        bool CanHandleTrigger(TTrigger trigger, bool exactMatch = false);
        bool CanHandleTrigger(TTrigger trigger, Type argumentType);
        bool CanHandleTrigger<TArgument>(TTrigger trigger);
    }

    [ContractClassFor(typeof (IStateMachine<,>))]
    public abstract class StateMachineContract<TState, TTrigger> : IStateMachine<TState, TTrigger>
    {
        public abstract IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        public abstract void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default);

        public void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument)
        {
            Contract.Requires<ArgumentNullException>(parameterizedTrigger != null);
        }

        public abstract void Fire(TTrigger trigger);
        public abstract IStateMachineDiagnostics<TState, TTrigger> Diagnostics { get; }
        public abstract event Action<TransitionExecutedEventArgs<TState, TTrigger>> TransitionExecuted;
        public abstract event Action<TriggerStateEventArgs<TState, TTrigger>> UnhandledTrigger;
        public abstract event Action<TransitionEventArgs<TState, TTrigger>> InvalidState;
        public abstract event Action<TransitionEventArgs<TState, TTrigger>> TransitionStarted;
        public abstract void Pause();
        public abstract void Resume();
        public abstract TState CurrentState { get; }
        public abstract bool IsEnabled { get; }
    }

    [ContractClassFor(typeof (IStateMachineDiagnostics<,>))]
    public abstract class StateMachineDiagnosticsContract<TState, TTrigger> :
        IStateMachineDiagnostics<TState, TTrigger>
    {
        public abstract bool CanHandleTrigger(TTrigger trigger, bool exactMatch = false);

        public bool CanHandleTrigger(TTrigger trigger, Type argumentType)
        {
            Contract.Requires<ArgumentNullException>(argumentType != null);
            return false;
        }

        public abstract bool CanHandleTrigger<TArgument>(TTrigger trigger);
        public abstract IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
    }
}
