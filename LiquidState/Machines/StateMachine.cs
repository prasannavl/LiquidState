// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LiquidState.Common;
using LiquidState.Configuration;
using LiquidState.Representations;

namespace LiquidState.Machines
{
    public class StateMachine<TState, TTrigger>
    {
        internal StateRepresentation<TState, TTrigger> currentStateRepresentation;

        internal StateMachine(TState initialState, StateMachineConfiguration<TState, TTrigger> configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);

            currentStateRepresentation = configuration.GetStateRepresentation(initialState);
            if (currentStateRepresentation == null)
            {
                throw new InvalidOperationException("StateMachine has no state");
            }

            IsEnabled = true;
        }

        public TState CurrentState
        {
            get { return currentStateRepresentation.State; }
        }

        public IEnumerable<TTrigger> CurrentPermittedTriggers
        {
            get
            {
                foreach (var triggerRepresentation in currentStateRepresentation.Triggers)
                {
                    yield return triggerRepresentation.Trigger;
                }
            }
        }

        public bool IsEnabled { get; private set; }
        public event Action<TTrigger, TState> UnhandledTriggerExecuted;
        public event Action<TState, TState> StateChanged;

        public bool CanHandleTrigger(TTrigger trigger)
        {
            foreach (var current in currentStateRepresentation.Triggers)
            {
                if (current.Equals(trigger))
                    return true;
            }

            return false;
        }

        public bool CanTransitionTo(TState state)
        {
            foreach (var current in currentStateRepresentation.Triggers)
            {
                if (current.NextStateRepresentation.State.Equals(state))
                    return true;
            }

            return false;
        }

        public void Pause()
        {
            IsEnabled = false;
        }

        public void Resume()
        {
            IsEnabled = true;
        }

        public void Stop()
        {
            IsEnabled = false;

            var currentExit = currentStateRepresentation.OnExitAction;
            ExecuteAction(currentExit);
        }

        private void ExecuteAction(Action action)
        {
            if (action != null)
                action();
        }

        private void HandleInvalidTrigger(TTrigger trigger)
        {
            var handler = UnhandledTriggerExecuted;
            if (handler != null)
                handler(trigger, currentStateRepresentation.State);
        }

        public void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument)
        {
            if (IsEnabled)
            {
                var trigger = parameterizedTrigger.Trigger;
                var triggerRep = StateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    currentStateRepresentation);

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
                catch (InvalidCastException e)
                {
                    InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                    return;
                }


                // Current exit
                var currentExit = currentStateRepresentation.OnExitAction;
                ExecuteAction(currentExit);

                // Trigger entry
                if (triggerAction != null)
                    triggerAction(argument);


                var nextStateRep = triggerRep.NextStateRepresentation;

                // Next entry
                var nextEntry = nextStateRep.OnEntryAction;
                ExecuteAction(nextEntry);

                currentStateRepresentation = nextStateRep;

                // Raise state change event
                var stateChangedHandler = StateChanged;
                if (stateChangedHandler != null)
                {
                    stateChangedHandler(previousState, currentStateRepresentation.State);
                }
            }
        }

        public void Fire(TTrigger trigger)
        {
            if (IsEnabled)
            {
                var triggerRep = StateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    currentStateRepresentation);

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
                catch (InvalidCastException e)
                {
                    InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                    return;
                }


                // Current exit
                var currentExit = currentStateRepresentation.OnExitAction;
                ExecuteAction(currentExit);

                // Trigger entry
                ExecuteAction(triggerAction);

                var nextStateRep = triggerRep.NextStateRepresentation;

                // Next entry
                var nextEntry = nextStateRep.OnEntryAction;
                ExecuteAction(nextEntry);

                currentStateRepresentation = nextStateRep;

                // Raise state change event
                var stateChangedHandler = StateChanged;
                if (stateChangedHandler != null)
                {
                    stateChangedHandler(previousState, currentStateRepresentation.State);
                }
            }
        }
    }
}