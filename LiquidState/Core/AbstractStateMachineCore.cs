// Author: Prasanna V. Loganathar
// Created: 12:20 18-06-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Threading;

namespace LiquidState.Core
{
    public abstract class AbstractStateMachineCore<TState, TTrigger> : IStateMachineCore<TState, TTrigger>
    {
        private int isEnabled = 1;
        public event Action<TriggerStateEventArgs<TState, TTrigger>> UnhandledTrigger;
        public event Action<TransitionEventArgs<TState, TTrigger>> InvalidState;
        public event Action<TransitionEventArgs<TState, TTrigger>> TransitionStarted;
        public event Action<TransitionExecutedEventArgs<TState, TTrigger>> TransitionExecuted;

        public virtual void Pause()
        {
            Interlocked.Exchange(ref isEnabled, 0);
        }

        public virtual void Resume()
        {
            Interlocked.Exchange(ref isEnabled, 1);
        }

        public abstract TState CurrentState { get; }

        public bool IsEnabled
        {
            get { return Interlocked.CompareExchange(ref isEnabled, -1, -1) == 1; }
        }

        public void RaiseInvalidTrigger(TTrigger trigger)
        {
            var handler = UnhandledTrigger;
            if (handler != null) handler.Invoke(new TriggerStateEventArgs<TState, TTrigger>(CurrentState, trigger));
        }

        public void RaiseInvalidState(TState targetState)
        {
            var handler = InvalidState;
            if (handler != null)
                handler.Invoke(new TransitionEventArgs<TState, TTrigger>(CurrentState, targetState));
        }

        public void RaiseInvalidState(TState targetState, TTrigger trigger)
        {
            var handler = InvalidState;
            if (handler != null)
                handler.Invoke(new TransitionEventArgs<TState, TTrigger>(CurrentState, targetState, trigger));
        }

        public void RaiseTransitionStarted(TState targetState)
        {
            var handler = TransitionStarted;
            if (handler != null)
                handler.Invoke(new TransitionEventArgs<TState, TTrigger>(CurrentState, targetState));
        }

        public void RaiseTransitionStarted(TState targetState, TTrigger trigger)
        {
            var handler = TransitionStarted;
            if (handler != null)
                handler.Invoke(new TransitionEventArgs<TState, TTrigger>(CurrentState, targetState, trigger));
        }

        public void RaiseTransitionExecuted(TState pastState)
        {
            var handler = TransitionExecuted;
            if (handler != null)
                handler.Invoke(new TransitionExecutedEventArgs<TState, TTrigger>(CurrentState, pastState));
        }

        public void RaiseTransitionExecuted(TState pastState, TTrigger trigger)
        {
            var handler = TransitionExecuted;
            if (handler != null)
                handler.Invoke(new TransitionExecutedEventArgs<TState, TTrigger>(CurrentState, pastState, trigger));
        }
    }
}
