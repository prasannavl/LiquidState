// Author: Prasanna V. Loganathar
// Created: 12:32 18-06-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiquidState.Awaitable.Core;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Awaitable
{
    public abstract class RawAwaitableStateMachineBase<TState, TTrigger> : AbstractStateMachineCore<TState, TTrigger>,
        IAwaitableStateMachine<TState, TTrigger>
    {
        private readonly RawAwaitableStateMachineDiagnostics<TState, TTrigger> m_diagnostics;
        internal AwaitableStateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        internal Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> Representations;

        protected RawAwaitableStateMachineBase(TState initialState,
            AwaitableConfiguration<TState, TTrigger> awaitableConfiguration)
        {
            CurrentStateRepresentation = awaitableConfiguration.GetInitialStateRepresentation(initialState);
            if (CurrentStateRepresentation == null) { ExceptionHelper.ThrowInvalidState(initialState); }

            Representations = awaitableConfiguration.Representations;
            m_diagnostics = new RawAwaitableStateMachineDiagnostics<TState, TTrigger>(this);
        }

        public virtual Task MoveToStateAsync(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            return !IsEnabled
                ? TaskHelpers.CompletedTask
                : AwaitableExecutionHelper.MoveToStateCoreAsync(state, option, this);
        }

        public virtual Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            return !IsEnabled
                ? TaskHelpers.CompletedTask
                : AwaitableExecutionHelper.FireCoreAsync(parameterizedTrigger, argument, this);
        }

        public virtual Task FireAsync(TTrigger trigger)
        {
            return !IsEnabled ? TaskHelpers.CompletedTask : AwaitableExecutionHelper.FireCoreAsync(trigger, this);
        }

        public IAwaitableStateMachineDiagnostics<TState, TTrigger> Diagnostics => m_diagnostics;

        public override TState CurrentState => CurrentStateRepresentation.State;
    }

    public class RawAwaitableStateMachineDiagnostics<TState, TTrigger> :
        IAwaitableStateMachineDiagnostics<TState, TTrigger>
    {
        private readonly RawAwaitableStateMachineBase<TState, TTrigger> machine;

        public RawAwaitableStateMachineDiagnostics(RawAwaitableStateMachineBase<TState, TTrigger> machine)
        {
            this.machine = machine;
        }

        public Task<bool> CanHandleTriggerAsync(TTrigger trigger, bool exactMatch = false)
        {
            return AwaitableDiagnosticsHelper.CanHandleTriggerAsync(trigger, machine, exactMatch);
        }

        public Task<bool> CanHandleTriggerAsync(TTrigger trigger, Type argumentType)
        {
            return AwaitableDiagnosticsHelper.CanHandleTriggerAsync(trigger, machine, argumentType);
        }

        public Task<bool> CanHandleTriggerAsync<TArgument>(TTrigger trigger)
        {
            return AwaitableDiagnosticsHelper.CanHandleTriggerAsync<TState, TTrigger, TArgument>(trigger, machine);
        }

        public IEnumerable<TTrigger> CurrentPermittedTriggers
            => AwaitableDiagnosticsHelper.EnumeratePermittedTriggers(machine);
    }


    public sealed class RawAwaitableStateMachine<TState, TTrigger> : RawAwaitableStateMachineBase<TState, TTrigger>
    {
        public RawAwaitableStateMachine(TState initialState,
            AwaitableConfiguration<TState, TTrigger> awaitableConfiguration)
            : base(initialState, awaitableConfiguration) {}
    }
}