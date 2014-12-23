using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Common;

namespace LiquidState.Machines
{
    [ContractClass(typeof(AwaitableStateMachineContract<,>))]
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

    [ContractClassFor(typeof(IAwaitableStateMachine<,>))]
    public abstract class AwaitableStateMachineContract<T, U> : IAwaitableStateMachine<T, U>
    {
        public abstract T CurrentState { get; }
        public abstract IEnumerable<U> CurrentPermittedTriggers { get; }
        public abstract bool IsInTransition { get; }
        public abstract bool IsEnabled { get; }
        public abstract bool CanHandleTrigger(U trigger);
        public abstract bool CanTransitionTo(T state);
        public abstract void Pause();
        public abstract void Resume();
        public abstract Task Stop();
        public abstract event Action<U, T> UnhandledTriggerExecuted;
        public abstract event Action<T, T> StateChanged;

        public Task FireAsync<TArgument>(ParameterizedTrigger<U, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            Contract.Requires<NullReferenceException>(parameterizedTrigger != null);
            return default(Task);
        }
        public abstract Task FireAsync(U trigger);
    }
}