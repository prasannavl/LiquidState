// Author: Prasanna V. Loganathar
// Created: 09:55 16-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using LiquidState.Core;
using LiquidState.Synchronous.Core;

namespace LiquidState.Synchronous
{
    public abstract class RawStateMachineBase<TState, TTrigger> : AbstractStateMachineCore<TState, TTrigger>,
        IStateMachine<TState, TTrigger>
    {
        private readonly RawStateMachineDiagnostics<TState, TTrigger> m_diagnostics;
        internal StateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        internal Dictionary<TState, StateRepresentation<TState, TTrigger>> Representations;

        protected RawStateMachineBase(TState initialState, Configuration<TState, TTrigger> configuration)
        {
            Representations = configuration.Representations;

            CurrentStateRepresentation = StateConfigurationHelper.FindStateRepresentation(
                initialState, Representations);

            if (CurrentStateRepresentation == null) { ExceptionHelper.ThrowInvalidState(initialState); }

            m_diagnostics = new RawStateMachineDiagnostics<TState, TTrigger>(this);
        }

        public virtual void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            if (!IsEnabled) return;
            ExecutionHelper.MoveToStateCore(state, option, this);
        }

        public virtual void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (!IsEnabled) return;
            ExecutionHelper.FireCore(parameterizedTrigger, argument, this);
        }

        public virtual void Fire(TTrigger trigger)
        {
            if (!IsEnabled) return;
            ExecutionHelper.FireCore(trigger, this);
        }

        public IStateMachineDiagnostics<TState, TTrigger> Diagnostics => m_diagnostics;
        public override TState CurrentState => CurrentStateRepresentation.State;
    }

    public class RawStateMachineDiagnostics<TState, TTrigger> : IStateMachineDiagnostics<TState, TTrigger>
    {
        private readonly RawStateMachineBase<TState, TTrigger> m_machine;

        public RawStateMachineDiagnostics(RawStateMachineBase<TState, TTrigger> machine)
        {
            m_machine = machine;
        }

        public bool CanHandleTrigger(TTrigger trigger, bool exactMatch = false)
        {
            return DiagnosticsHelper.CanHandleTrigger(trigger, m_machine, exactMatch);
        }

        public bool CanHandleTrigger(TTrigger trigger, Type argumentType)
        {
            return DiagnosticsHelper.CanHandleTrigger(trigger, m_machine, argumentType);
        }

        public bool CanHandleTrigger<TArgument>(TTrigger trigger)
        {
            return DiagnosticsHelper.CanHandleTrigger<TState, TTrigger, TArgument>(trigger, m_machine);
        }

        public IEnumerable<TTrigger> CurrentPermittedTriggers => DiagnosticsHelper.EnumeratePermittedTriggers(m_machine)
            ;
    }

    public sealed class RawStateMachine<TState, TTrigger> : RawStateMachineBase<TState, TTrigger>
    {
        public RawStateMachine(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration) {}
    }
}