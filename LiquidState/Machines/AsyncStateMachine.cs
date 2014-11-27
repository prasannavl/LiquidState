// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using LiquidState.Configuration;
using LiquidState.Representations;

namespace LiquidState.Machines
{
    public class AsyncStateMachine<TState, TTrigger>
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
            get { return currentStateRepresentation.Triggers.Select(x => x.Trigger); }
        }

        public bool IsEnabled { get; private set; }

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

        public async Task Stop()
        {
            IsEnabled = false;

            var current = currentStateRepresentation;
            if ((current.TransitionFlags & AsyncStateTransitionFlag.ExitReturnsTask) ==
                AsyncStateTransitionFlag.ExitReturnsTask)
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

        public event Action<TState, TTrigger> UnhandledTriggerExecuted;
        public event Action<TState, TState> StateChanged;

        public async Task Fire(TTrigger trigger, object parameter = null)
        {
            if (IsEnabled)
            {
                var triggerRep = AsyncStateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    currentStateRepresentation);
                if (triggerRep == null)
                {
                    var h = UnhandledTriggerExecuted;
                    if (h != null)
                        h(currentStateRepresentation.State, trigger);
                    return;
                }

                if ((triggerRep.TransitionFlags & AsyncStateTransitionFlag.TriggerPredicateReturnsTask) ==
                    AsyncStateTransitionFlag.ExitReturnsTask)
                {
                    var exit = triggerRep.ConditionalTriggerPredicate as Func<Task<bool>>;
                    if (exit != null)
                        if (!await exit()) return;
                }
                else
                {
                    var exit = triggerRep.ConditionalTriggerPredicate as Func<bool>;
                    if (exit != null)
                        if (!exit()) return;
                }

                // Current exit

                if ((currentStateRepresentation.TransitionFlags & AsyncStateTransitionFlag.ExitReturnsTask) ==
                    AsyncStateTransitionFlag.ExitReturnsTask)
                {
                    var exit = currentStateRepresentation.OnExitAction as Func<Task>;
                    if (exit != null)
                        await exit();
                }
                else
                {
                    var exit = currentStateRepresentation.OnExitAction as Action;
                    if (exit != null)
                        exit();
                }

                // Trigger entry

                if ((triggerRep.TransitionFlags & AsyncStateTransitionFlag.TriggerActionReturnsTask) ==
                    AsyncStateTransitionFlag.TriggerActionReturnsTask)
                {
                    if (triggerRep.OnTriggerAction != null)
                    {
                        if (triggerRep.WrappedTriggerAction == null)
                        {
                            var func = (Func<Task>) triggerRep.OnTriggerAction;

                            Contract.Assert(func != null);
                            // Will never be null if wrapper is not null => Enforced on creation.
                            // ReSharper disable once PossibleNullReferenceException
                            await func();
                        }
                        else
                        {
                            var wrappedFunc = (Func<object, Task>) triggerRep.WrappedTriggerAction;

                            Contract.Assert(wrappedFunc != null);
                            // Will never be null if wrapper is not null => Enforced on creation.
                            // ReSharper disable once PossibleNullReferenceException
                            await wrappedFunc(parameter);
                        }
                    }
                }
                else
                {
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

                // Next state entry

                var nextStateRep = triggerRep.NextStateRepresentation;

                if ((nextStateRep.TransitionFlags & AsyncStateTransitionFlag.EntryReturnsTask) ==
                    AsyncStateTransitionFlag.EntryReturnsTask)
                {
                    var entry = nextStateRep.OnEntryAction as Func<Task>;
                    if (entry != null)
                        await entry();
                }
                else
                {
                    var entry = nextStateRep.OnEntryAction as Action;
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