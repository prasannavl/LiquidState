// Author: Prasanna V. Loganathar
// Created: 12:32 18-06-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;

namespace LiquidState.Core
{
    public interface IStateMachineCore<TState, TTrigger>
    {
        TState CurrentState { get; }
        bool IsEnabled { get; }
        event Action<TransitionEventArgs<TState, TTrigger>> InvalidState;
        event Action<TransitionExecutedEventArgs<TState, TTrigger>> TransitionExecuted;
        event Action<TransitionEventArgs<TState, TTrigger>> TransitionStarted;
        event Action<TriggerStateEventArgs<TState, TTrigger>> UnhandledTrigger;
        void Pause();
        void Resume();
    }

    public interface IStateMachineDiagnosticsCore<TState, out TTrigger>
    {
        IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
    }
}