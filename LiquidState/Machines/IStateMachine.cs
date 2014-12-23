using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LiquidState.Common;

namespace LiquidState.Machines
{
    [ContractClass(typeof(StateMachineContract<,>))]
    public interface IStateMachine<TState, TTrigger>
    {
        TState CurrentState { get; }
        IEnumerable<TTrigger> CurrentPermittedTriggers { get; }
        bool IsEnabled { get; }
        event Action<TTrigger, TState> UnhandledTriggerExecuted;
        event Action<TState, TState> StateChanged;
        bool IsInTransition { get; }
        bool CanHandleTrigger(TTrigger trigger);
        bool CanTransitionTo(TState state);
        void Pause();
        void Resume();
        void Stop();
        void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument);
        void Fire(TTrigger trigger);
    }

    [ContractClassFor(typeof(IStateMachine<,>))]
    public abstract class StateMachineContract<T, U> : IStateMachine<T,U>
    {
        public abstract T CurrentState { get; }
        public abstract IEnumerable<U> CurrentPermittedTriggers { get; }
        public abstract bool IsEnabled { get; }
        public abstract event Action<U, T> UnhandledTriggerExecuted;
        public abstract event Action<T, T> StateChanged;
        public abstract bool IsInTransition { get; }
        public abstract bool CanHandleTrigger(U trigger);
        public abstract bool CanTransitionTo(T state);
        public abstract void Pause();
        public abstract void Resume();
        public abstract void Stop();

        public void Fire<TArgument>(ParameterizedTrigger<U, TArgument> parameterizedTrigger, TArgument argument)
        {
            Contract.Requires<NullReferenceException>(parameterizedTrigger != null);
        }
        public abstract void Fire(U trigger);
    }
}