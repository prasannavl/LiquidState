// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using LiquidState.Common;
using LiquidState.Configuration;
using LiquidState.Representations;

namespace LiquidState.Machines
{
    public class BlockingStateMachine<TState, TTrigger> : IStateMachine<TState, TTrigger>
    {
        internal StateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        private readonly Dictionary<TState, StateRepresentation<TState, TTrigger>> configDictionary;
        private readonly object syncRoot = new object();
        private int isEnabled = 1;

        internal BlockingStateMachine(TState initialState, StateMachineConfiguration<TState, TTrigger> configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);

            CurrentStateRepresentation = configuration.GetInitialStateRepresentation(initialState);
            if (CurrentStateRepresentation == null)
            {
                throw new InvalidOperationException("StateMachine has an unreachable state");
            }

            configDictionary = configuration.Config;
        }

        public event Action<TTrigger, TState> UnhandledTriggerExecuted;
        public event Action<TState, TState> StateChanged;

        public void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            lock (syncRoot)
            {
                if (!IsEnabled) return;
                StateRepresentation<TState, TTrigger> rep;
                if (configDictionary.TryGetValue(state, out rep))
                {
                    if ((option & StateTransitionOption.CurrentStateExitTransition) ==
                        StateTransitionOption.CurrentStateExitTransition)
                    {
                        ExecuteAction(CurrentStateRepresentation.OnExitAction);
                    }
                    if ((option & StateTransitionOption.NewStateEntryTransition) ==
                        StateTransitionOption.NewStateEntryTransition)
                    {
                        ExecuteAction(rep.OnEntryAction);
                    }

                    CurrentStateRepresentation = rep;
                }
                else
                {
                    throw new InvalidOperationException("Invalid state: " + state.ToString());
                }
            }
        }

        public bool CanHandleTrigger(TTrigger trigger)
        {
            foreach (var current in CurrentStateRepresentation.Triggers)
            {
                if (current.Trigger.Equals(trigger))
                {
                    var predicate = current.ConditionalTriggerPredicate;
                    return predicate == null || predicate();
                }
            }
            return false;
        }

        public bool CanTransitionTo(TState state)
        {
            foreach (var current in CurrentStateRepresentation.Triggers)
            {
                if (current.NextStateRepresentation.State.Equals(state))
                    return true;
            }

            return false;
        }

        public void Pause()
        {
            Interlocked.Exchange(ref isEnabled, 0);
        }

        public void Resume()
        {
            Interlocked.Exchange(ref isEnabled, 1);
        }

        public void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument)
        {
            lock (syncRoot)
            {
                if (!IsEnabled) return;
                var trigger = parameterizedTrigger.Trigger;
                var triggerRep = StateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    CurrentStateRepresentation);

                if (triggerRep == null)
                {
                    HandleInvalidTrigger(trigger);
                    return;
                }

                var previousState = CurrentState;

                var predicate = triggerRep.ConditionalTriggerPredicate;
                if (predicate != null)
                {
                    if (!predicate())
                    {
                        HandleInvalidTrigger(trigger);
                        return;
                    }
                }

                // Handle ignored trigger

                if (triggerRep.NextStateRepresentation == null)
                {
                    return;
                }

                // Catch invalid paramters before execution.

                Action<TArgument> triggerAction = null;
                try
                {
                    triggerAction = (Action<TArgument>) triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                    return;
                }


                // Current exit
                var currentExit = CurrentStateRepresentation.OnExitAction;
                ExecuteAction(currentExit);

                // Trigger entry
                if (triggerAction != null) triggerAction.Invoke(argument);


                var nextStateRep = triggerRep.NextStateRepresentation;

                // Next entry
                var nextEntry = nextStateRep.OnEntryAction;
                ExecuteAction(nextEntry);

                CurrentStateRepresentation = nextStateRep;

                // Raise state change event
                var stateChangedHandler = StateChanged;
                if (stateChangedHandler != null)
                    stateChangedHandler.Invoke(previousState, CurrentStateRepresentation.State);
            }
        }

        public void Fire(TTrigger trigger)
        {
            lock (syncRoot)
            {
                if (!IsEnabled) return;
                var triggerRep = StateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    CurrentStateRepresentation);

                if (triggerRep == null)
                {
                    HandleInvalidTrigger(trigger);
                    return;
                }

                var previousState = CurrentState;

                var predicate = triggerRep.ConditionalTriggerPredicate;
                if (predicate != null)
                {
                    if (!predicate())
                    {
                        HandleInvalidTrigger(trigger);
                        return;
                    }
                }

                // Handle ignored trigger

                if (triggerRep.NextStateRepresentation == null)
                {
                    return;
                }

                // Catch invalid paramters before execution.

                Action triggerAction = null;
                try
                {
                    triggerAction = (Action) triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                    return;
                }


                // Current exit
                var currentExit = CurrentStateRepresentation.OnExitAction;
                ExecuteAction(currentExit);

                // Trigger entry
                ExecuteAction(triggerAction);

                var nextStateRep = triggerRep.NextStateRepresentation;

                // Next entry
                var nextEntry = nextStateRep.OnEntryAction;
                ExecuteAction(nextEntry);

                CurrentStateRepresentation = nextStateRep;

                // Raise state change event
                var stateChangedHandler = StateChanged;
                if (stateChangedHandler != null)
                    stateChangedHandler.Invoke(previousState, CurrentStateRepresentation.State);
            }
        }

        public bool IsInTransition
        {
            get { return Monitor.IsEntered(syncRoot); }
        }

        public TState CurrentState
        {
            get { return CurrentStateRepresentation.State; }
        }

        public IEnumerable<TTrigger> CurrentPermittedTriggers
        {
            get
            {
                foreach (var triggerRepresentation in CurrentStateRepresentation.Triggers)
                {
                    yield return triggerRepresentation.Trigger;
                }
            }
        }

        public bool IsEnabled
        {
            get { return Interlocked.CompareExchange(ref isEnabled, -1, -1) == 1; }
        }

        private void ExecuteAction(Action action)
        {
            if (action != null) action.Invoke();
        }

        private void HandleInvalidTrigger(TTrigger trigger)
        {
            var handler = UnhandledTriggerExecuted;
            if (handler != null) handler.Invoke(trigger, CurrentStateRepresentation.State);
        }
    }
}
