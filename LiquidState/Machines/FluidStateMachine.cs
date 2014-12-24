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
    public class FluidStateMachine<TState, TTrigger>
    {
        internal FluidStateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        private FluidStateMachineConfiguration<TState, TTrigger> configuration;
        private volatile bool isRunning;

        internal FluidStateMachine(TState initialState, FluidStateMachineConfiguration<TState, TTrigger> configuration)
        {
            Contract.Requires(configuration != null);

            CurrentStateRepresentation = configuration.GetInitialStateRepresentation(initialState) ??
                                         new FluidStateRepresentation<TState, TTrigger>(default(TState));

            this.configuration = configuration;

            IsEnabled = true;
        }

        public bool IsInTransition
        {
            get { return isRunning; }
        }

        public TState CurrentState
        {
            get { return CurrentStateRepresentation.State; }
        }

        public IEnumerable<TTrigger> CurrentAvailableTriggers
        {
            get
            {
                foreach (var triggerRepresentation in CurrentStateRepresentation.Triggers)
                {
                    yield return triggerRepresentation.Trigger;
                }
            }
        }

        public bool IsEnabled { get; private set; }
        public bool IsFluidFlowActive { get; private set; }
        public bool AcceptExternalStates { get; private set; }

        public void EnableFluidFlow(bool acceptExternalStates = false)
        {
            IsFluidFlowActive = true;
            AcceptExternalStates = acceptExternalStates;
        }

        public void DisableFluidFlow()
        {
            IsFluidFlowActive = AcceptExternalStates = false;
        }

        public event Action<TTrigger, TState> UnhandledTriggerExecuted;
        public event Action<TState, TState> StateChanged;

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
            IsEnabled = false;
        }

        public void Resume()
        {
            IsEnabled = true;
        }

        public void Stop()
        {
            IsEnabled = false;

            var currentExit = CurrentStateRepresentation.OnExitAction;
            ExecuteAction(currentExit);
        }

        public void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument)
        {
            Contract.Requires<ArgumentNullException>(parameterizedTrigger != null);

            if (isRunning)
                throw new InvalidOperationException("State cannot be changed while in transition");

            if (IsEnabled)
            {
                isRunning = true;

                try
                {
                    var trigger = parameterizedTrigger.Trigger;
                    var triggerRep = FluidStateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
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
                finally
                {
                    isRunning = false;
                }
            }
        }

        public void MoveToState(TState targetState, bool useTriggerIfAvailable = true)
        {
            if (isRunning)
                throw new InvalidOperationException("State cannot be changed while in transition");

            if (IsEnabled)
            {
                isRunning = true;

                try
                {
                    if (useTriggerIfAvailable)
                    {
                        var triggerRep =
                            FluidStateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentationForTargetState(
                                targetState, CurrentStateRepresentation);

                        // No triggers found. Use Fluid change flow if possible.
                        if (triggerRep == null)
                        {
                            MoveToStateInternal(targetState, true, null);
                            return;
                        }
                        else
                        {
                            Action triggerAction = null;
                            if (triggerRep.OnTriggerAction != null)
                            {
                                triggerAction = triggerRep.OnTriggerAction as Action;

                                if (triggerAction == null)
                                {
                                    // Uses parameterized argument. Not possible to switch to state without trigger arguments. Use Fluid change flow.
                                    MoveToStateInternal(targetState, true, null);
                                    return;
                                }

                                MoveToStateInternal(targetState, false, triggerRep.NextStateRepresentation,
                                    triggerAction);
                            }

                            MoveToStateInternal(targetState, false, triggerRep.NextStateRepresentation, triggerAction);
                        }
                    }
                    else
                    {
                        MoveToStateInternal(targetState, true, null);
                    }
                }
                finally
                {
                    isRunning = false;
                }
            }
        }

        private FluidStateRepresentation<TState, TTrigger> GetStateRepresentation(TState targetState)
        {
            FluidStateRepresentation<TState, TTrigger> result = null;
            if (configuration.config.TryGetValue(targetState, out result))
            {
                return result;
            }
            if (AcceptExternalStates)
                return null;

            throw new InvalidOperationException("Invalid state transition for fluid flow attempt");
        }

        private void MoveToStateInternal(TState targetState, bool isFluidFlow,
            FluidStateRepresentation<TState, TTrigger> targetStateRep, Action triggerAction = null)
        {
            if (isFluidFlow)
            {
                HandleFlowFlags();
                if (targetStateRep == null)
                {
                    targetStateRep = GetStateRepresentation(targetState) ??
                                     new FluidStateRepresentation<TState, TTrigger>(default(TState));
                }
            }
            else
            {
                // Make sure the Ignore triggers are honoured, acting as a blacklist in ForceFluidFlow state.
                if (targetStateRep == null)
                    return;
            }

            // Current exit
            var currentExit = CurrentStateRepresentation.OnExitAction;
            ExecuteAction(currentExit);

            // Trigger entry
            if (triggerAction != null) triggerAction.Invoke();

            // Next entry
            var nextEntry = targetStateRep.OnEntryAction;
            ExecuteAction(nextEntry);

            var previousState = CurrentStateRepresentation.State;
            CurrentStateRepresentation = targetStateRep;

            // Raise state change event
            var stateChangedHandler = StateChanged;
            if (stateChangedHandler != null)
                stateChangedHandler.Invoke(previousState, CurrentStateRepresentation.State);
        }

        private void HandleFlowFlags()
        {
            var flags = CurrentStateRepresentation.TransitionFlags;
            var isFluidOverride = flags.HasFlag(FluidTransitionFlag.OverrideFluidState);
            var isFluidFlowOverrideActive = flags.HasFlag(FluidTransitionFlag.FluidStateActive);

            if (isFluidOverride)
            {
                if (isFluidFlowOverrideActive)
                    return;
                else
                    throw new InvalidOperationException(
                        "Invalid state. Fluid flow is enabled but explicitly overriden to be disabled for current state to switch freely.");
            }
            if (!IsFluidFlowActive)
            {
                throw new InvalidOperationException(
                    "Invalid state. Fluid flow is not enabled for switching to any state.");
            }
        }

        public void Fire(TTrigger trigger)
        {
            if (isRunning)
                throw new InvalidOperationException("State cannot be changed while in transition");

            if (IsEnabled)
            {
                isRunning = true;

                try
                {
                    var triggerRep = FluidStateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
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
                finally
                {
                    isRunning = false;
                }
            }
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