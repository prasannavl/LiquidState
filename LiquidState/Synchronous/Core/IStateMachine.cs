// Author: Prasanna V. Loganathar
// Created: 12:32 18-06-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LiquidState.Core;

namespace LiquidState.Synchronous.Core
{
    public interface IStateMachine<TState, TTrigger> : IStateMachineCore<TState, TTrigger>
    {
        IStateMachineDiagnostics<TState, TTrigger> Diagnostics { get; }
        void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument);
        void Fire(TTrigger trigger);
        void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default);
    }

    public interface IStateMachineDiagnostics<TState, TTrigger> : IStateMachineDiagnosticsCore<TState, TTrigger>
    {
        bool CanHandleTrigger(TTrigger trigger, bool exactMatch = false);
        bool CanHandleTrigger(TTrigger trigger, Type argumentType);
        bool CanHandleTrigger<TArgument>(TTrigger trigger);
    }
}