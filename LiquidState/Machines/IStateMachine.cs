using System;
using System.Collections.Generic;
using LiquidState.Common;

namespace LiquidState.Machines
{
    public interface IStateMachine<TState, TTrigger>
    {
        TState CurrentState { get; }
        IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        bool IsEnabled { get; }
        event Action<TTrigger, TState> UnhandledTriggerExecuted;
        event Action<TState, TState> StateChanged;
        bool CanHandleTrigger(TTrigger trigger);
        bool CanTransitionTo(TState state);
        void Pause();
        void Resume();
        void Stop();
        void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument);
        void Fire(TTrigger trigger);
    }
}