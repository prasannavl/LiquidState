using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiquidState.Common;

namespace LiquidState.Machines
{
    public interface IAwaitableStateMachine<TState, TTrigger>
    {
        TState CurrentState { get; }
        IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        bool IsInTransition { get; }
        bool IsEnabled { get; }
        bool CanHandleTrigger(TTrigger trigger);
        bool CanTransitionTo(TState state);
        void Pause();
        void Resume();
        Task Stop();
        event Action<TTrigger, TState> UnhandledTriggerExecuted;
        event Action<TState, TState> StateChanged;

        Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument);

        Task FireAsync(TTrigger trigger);
    }
}