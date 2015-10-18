// Author: Prasanna V. Loganathar
// Created: 12:32 18-06-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Core;

namespace LiquidState.Awaitable.Core
{
    public interface IAwaitableStateMachine<TState, TTrigger> : IStateMachineCore<TState, TTrigger>
    {
        IAwaitableStateMachineDiagnostics<TState, TTrigger> Diagnostics { get; }

        Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument);

        Task FireAsync(TTrigger trigger);
        Task MoveToStateAsync(TState state, StateTransitionOption option = StateTransitionOption.Default);
    }

    public interface IAwaitableStateMachineDiagnostics<TState, TTrigger> :
        IStateMachineDiagnosticsCore<TState, TTrigger>
    {
        Task<bool> CanHandleTriggerAsync(TTrigger trigger, bool exactMatch = false);
        Task<bool> CanHandleTriggerAsync(TTrigger trigger, Type argumentType);
        Task<bool> CanHandleTriggerAsync<TArgument>(TTrigger trigger);
    }
}