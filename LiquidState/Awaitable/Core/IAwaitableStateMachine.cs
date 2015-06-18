// Author: Prasanna V. Loganathar
// Created: 1:33 AM 05-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Core;

namespace LiquidState.Awaitable.Core
{
    [ContractClass(typeof (AwaitableStateMachineContract<,>))]
    public interface IAwaitableStateMachine<TState, TTrigger> : IStateMachineCore<TState, TTrigger>
    {

        Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument);

        Task FireAsync(TTrigger trigger);
        Task MoveToStateAsync(TState state, StateTransitionOption option = StateTransitionOption.Default);
    }

    [ContractClassFor(typeof(IAwaitableStateMachine<,>))]
    public abstract class AwaitableStateMachineContract<TState, TTrigger> : IAwaitableStateMachine<TState, TTrigger>
    {
        public abstract IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        public abstract Task MoveToStateAsync(TState state, StateTransitionOption option = StateTransitionOption.Default);

        public Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            Contract.Requires<ArgumentNullException>(parameterizedTrigger != null);
            return default(Task);
        }

        public abstract Task FireAsync(TTrigger trigger);
        public abstract Task<bool> CanHandleTriggerAsync(TTrigger trigger, bool exactMatch = false);

        public Task<bool> CanHandleTriggerAsync(TTrigger trigger, Type argumentType)
        {
            Contract.Requires<ArgumentNullException>(argumentType != null);
            return default(Task<bool>);
        }

        public abstract Task<bool> CanHandleTriggerAsync<TArgument>(TTrigger trigger);
        public abstract event Action<TransitionExecutedEventArgs<TState, TTrigger>> TransitionExecuted;
        public abstract event Action<TriggerStateEventArgs<TState, TTrigger>> UnhandledTrigger;
        public abstract event Action<TransitionEventArgs<TState, TTrigger>> InvalidState;
        public abstract event Action<TransitionEventArgs<TState, TTrigger>> TransitionStarted;
        public abstract void Pause();
        public abstract void Resume();
        public abstract TState CurrentState { get; }
        public abstract bool IsEnabled { get; }
    }
}
