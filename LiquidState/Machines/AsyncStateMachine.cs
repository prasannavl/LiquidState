// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LiquidState.Common;
using LiquidState.Configuration;
using LiquidState.Representations;

namespace LiquidState.Machines
{
    public class AsyncStateMachine<TState, TTrigger> : IAsyncStateMachine<TState, TTrigger>
    {
        internal AsyncStateRepresentation<TState, TTrigger> currentStateRepresentation;

        internal AsyncStateMachine(TState initialState, AsyncStateMachineConfiguration<TState, TTrigger> configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);

            currentStateRepresentation = configuration.GetStateRepresentation(initialState);
            if (currentStateRepresentation == null)
            {
                throw new InvalidOperationException("StateMachine has no states");
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

        public async Task Stop()
        {
            IsEnabled = false;

            var current = currentStateRepresentation;
            if (CheckFlag(current.TransitionFlags, AsyncStateTransitionFlag.ExitReturnsTask))
            {
                var exit = current.OnExitAction as Func<Task>;
                if (exit != null)
                    await exit();
            }
            else
            {
                var exit = current.OnExitAction as Action;
                if (exit != null)
                    exit();
            }
        }

        public event Action<TTrigger, TState> UnhandledTriggerExecuted;
        public event Action<TState, TState> StateChanged;

        private void HandleInvalidTrigger(TTrigger trigger)
        {
            var handler = UnhandledTriggerExecuted;
            if (handler != null)
                handler(trigger, currentStateRepresentation.State);
        }

        private bool CheckFlag(AsyncStateTransitionFlag source, AsyncStateTransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }

        public async Task Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (IsEnabled)
            {
                var trigger = parameterizedTrigger.Trigger;
                var triggerRep = AsyncStateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    currentStateRepresentation);

                if (triggerRep == null)
                {
                    HandleInvalidTrigger(trigger);
                    return;
                }

                if (CheckFlag(triggerRep.TransitionFlags, AsyncStateTransitionFlag.TriggerPredicateReturnsTask))
                {
                    var predicate = (Func<Task<bool>>) triggerRep.ConditionalTriggerPredicate;
                    if (predicate != null)
                        if (!await predicate())
                        {
                            HandleInvalidTrigger(trigger);
                            return;
                        }
                }
                else
                {
                    var predicate = (Func<bool>) triggerRep.ConditionalTriggerPredicate;
                    if (predicate != null)
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
                Func<TArgument, Task> triggerFunc = null;
                if (CheckFlag(triggerRep.TransitionFlags, AsyncStateTransitionFlag.TriggerActionReturnsTask))
                {
                    try
                    {
                        triggerFunc = (Func<TArgument, Task>) triggerRep.OnTriggerAction;
                    }
                    catch (InvalidCastException)
                    {
                        InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                        return;
                    }
                }
                else
                {
                    try
                    {
                        triggerAction = (Action<TArgument>) triggerRep.OnTriggerAction;
                    }
                    catch (InvalidCastException)
                    {
                        InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                        return;
                    }
                }

                // Current exit

                if (CheckFlag(triggerRep.TransitionFlags, AsyncStateTransitionFlag.ExitReturnsTask))
                {
                    var exit = (Func<Task>) currentStateRepresentation.OnExitAction;
                    if (exit != null)
                        await exit();
                }
                else
                {
                    var exit = (Action) currentStateRepresentation.OnExitAction;
                    if (exit != null)
                        exit();
                }

                // Trigger entry

                if (triggerAction != null)
                {
                    triggerAction(argument);
                }
                else if (triggerFunc != null)
                {
                    await triggerFunc(argument);
                }

                // Next state entry

                var nextStateRep = triggerRep.NextStateRepresentation;

                if (CheckFlag(nextStateRep.TransitionFlags, AsyncStateTransitionFlag.EntryReturnsTask))
                {
                    var entry = (Func<Task>) nextStateRep.OnEntryAction;
                    if (entry != null)
                        await entry();
                }
                else
                {
                    var entry = (Action) nextStateRep.OnEntryAction;
                    if (entry != null)
                        entry();
                }

                // Set states

                var previousState = currentStateRepresentation.State;
                currentStateRepresentation = nextStateRep;

                // Raise event

                var sc = StateChanged;
                if (sc != null)
                {
                    sc(previousState, currentStateRepresentation.State);
                }
            }
        }

        public async Task Fire(TTrigger trigger)
        {
            if (IsEnabled)
            {
                var triggerRep = AsyncStateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    currentStateRepresentation);

                if (triggerRep == null)
                {
                    HandleInvalidTrigger(trigger);
                    return;
                }

                if (CheckFlag(triggerRep.TransitionFlags, AsyncStateTransitionFlag.TriggerPredicateReturnsTask))
                {
                    var predicate = (Func<Task<bool>>) triggerRep.ConditionalTriggerPredicate;
                    if (predicate != null)
                        if (!await predicate())
                        {
                            HandleInvalidTrigger(trigger);
                            return;
                        }
                }
                else
                {
                    var predicate = (Func<bool>) triggerRep.ConditionalTriggerPredicate;
                    if (predicate != null)
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
                Func<Task> triggerFunc = null;
                if (CheckFlag(triggerRep.TransitionFlags, AsyncStateTransitionFlag.TriggerActionReturnsTask))
                {
                    try
                    {
                        triggerFunc = (Func<Task>) triggerRep.OnTriggerAction;
                    }
                    catch (InvalidCastException)
                    {
                        InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                        return;
                    }
                }
                else
                {
                    try
                    {
                        triggerAction = (Action) triggerRep.OnTriggerAction;
                    }
                    catch (InvalidCastException)
                    {
                        InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                        return;
                    }
                }

                // Current exit

                if (CheckFlag(triggerRep.TransitionFlags, AsyncStateTransitionFlag.ExitReturnsTask))
                {
                    var exit = (Func<Task>) currentStateRepresentation.OnExitAction;
                    if (exit != null)
                        await exit();
                }
                else
                {
                    var exit = (Action) currentStateRepresentation.OnExitAction;
                    if (exit != null)
                        exit();
                }

                // Trigger entry

                if (triggerAction != null)
                {
                    triggerAction();
                }
                else if (triggerFunc != null)
                {
                    await triggerFunc();
                }

                // Next state entry

                var nextStateRep = triggerRep.NextStateRepresentation;

                if (CheckFlag(nextStateRep.TransitionFlags, AsyncStateTransitionFlag.EntryReturnsTask))
                {
                    var entry = (Func<Task>) nextStateRep.OnEntryAction;
                    if (entry != null)
                        await entry();
                }
                else
                {
                    var entry = (Action) nextStateRep.OnEntryAction;
                    if (entry != null)
                        entry();
                }

                // Set states

                var previousState = currentStateRepresentation.State;
                currentStateRepresentation = nextStateRep;

                // Raise event

                var sc = StateChanged;
                if (sc != null)
                {
                    sc(previousState, currentStateRepresentation.State);
                }
            }
        }
    }
}