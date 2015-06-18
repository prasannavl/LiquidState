using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LiquidState.Common;

namespace LiquidState.Core
{
    public interface IStateMachineCore<TState, TTrigger>
    {
        TState CurrentState { get; }

        bool IsEnabled { get; }
        void Pause();
        void Resume();

        event Action<TriggerStateEventArgs<TState, TTrigger>> UnhandledTrigger;
        event Action<TransitionEventArgs<TState, TTrigger>> InvalidState;
        event Action<TransitionEventArgs<TState, TTrigger>> TransitionStarted;
        event Action<TransitionExecutedEventArgs<TState, TTrigger>> TransitionExecuted;
    }

    public interface IStateMachineDiagnosticsCore<TState, TTrigger>
    {
        IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
    }

}