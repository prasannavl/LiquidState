// Author: Prasanna V. Loganathar
// Created: 1:34 AM 05-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Common;

namespace LiquidState.Machines
{
    [ContractClass(typeof (AwaitableStateMachineContract<,>))]
    public interface IAwaitableStateMachine<TState, TTrigger>
    {
        IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        TState CurrentState { get; }
        bool IsEnabled { get; }
        bool IsInTransition { get; }
        event Action<TState, TState> StateChanged;
        event Action<TTrigger, TState> UnhandledTriggerExecuted;
        Task<bool> CanHandleTriggerAsync(TTrigger trigger);
        bool CanTransitionTo(TState state);

        Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument);

        Task FireAsync(TTrigger trigger);
        Task MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default);
        void Pause();
        void Resume();
    }

    [ContractClassFor(typeof (IAwaitableStateMachine<,>))]
    public abstract class AwaitableStateMachineContract<T, U> : IAwaitableStateMachine<T, U>
    {
        public abstract event Action<U, T> UnhandledTriggerExecuted;
        public abstract event Action<T, T> StateChanged;
        public abstract Task<bool> CanHandleTriggerAsync(U trigger);
        public abstract bool CanTransitionTo(T state);
        public abstract Task MoveToState(T state, StateTransitionOption option = StateTransitionOption.Default);
        public abstract void Pause();
        public abstract void Resume();

        public Task FireAsync<TArgument>(ParameterizedTrigger<U, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            Contract.Requires<NullReferenceException>(parameterizedTrigger != null);
            return default(Task);
        }

        public abstract Task FireAsync(U trigger);
        public abstract T CurrentState { get; }
        public abstract IEnumerable<U> CurrentPermittedTriggers { get; }
        public abstract bool IsInTransition { get; }
        public abstract bool IsEnabled { get; }
    }
}
