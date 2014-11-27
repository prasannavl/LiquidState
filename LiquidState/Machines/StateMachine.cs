// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
            get { return currentStateRepresentation.Triggers.Select(x => x.Trigger); }
        }

        public bool IsEnabled { get; private set; }
        public event Action<TState, TTrigger> UnhandledTriggerExecuted;
        public event Action<TState, TState> StateChanged;

        public bool CanHandleTrigger(TTrigger trigger)
        {
            return currentStateRepresentation.Triggers.Any(x => x.Trigger.Equals(trigger));
        }

        public bool CanTransitionTo(TState state)
        {
            return currentStateRepresentation.Triggers.Any(x => x.NextStateRepresentation.State.Equals(state));
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

        private void ExecuteTriggerAction(TriggerRepresentation<TTrigger, TState> triggerRep, object parameter)
        {
            Contract.Requires(triggerRep != null);

            if (triggerRep.OnTriggerAction != null)
            {
                if (triggerRep.WrappedTriggerAction == null)
                {
                    var action = (Action) triggerRep.OnTriggerAction;

                    Contract.Assert(action != null);
                    // Will never be null if wrapper is not null => Enforced on creation.
                    // ReSharper disable once PossibleNullReferenceException
                    action();
                }
                else
                {
                    var wrappedAction = (Action<object>) triggerRep.WrappedTriggerAction;

                    Contract.Assert(wrappedAction != null);
                    // Will never be null if wrapper is not null => Enforced on creation.
                    // ReSharper disable once PossibleNullReferenceException
                    wrappedAction(parameter);
                }
            }
        }

        public void Fire(TTrigger trigger, object parameter = null)
        {
            if (IsEnabled)
            {
                var triggerRep = StateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    currentStateRepresentation);

                if (triggerRep == null)
                {
                    var handler = UnhandledTriggerExecuted;
                    if (handler != null)
                        handler(currentStateRepresentation.State, trigger);
                    return;
                }

                var previousState = CurrentState;

                var predicate = triggerRep.ConditionalTriggerPredicate;
                if (predicate != null)
                {
                    if (!predicate())
                    {
                        return;
                    }
                }

                // Current exit
                var currentExit = currentStateRepresentation.OnExitAction;
                ExecuteAction(currentExit);

                // Trigger entry
                ExecuteTriggerAction(triggerRep, parameter);

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