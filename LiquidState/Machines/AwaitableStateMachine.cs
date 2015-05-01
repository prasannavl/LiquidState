// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
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
    public class AwaitableStateMachine<TState, TTrigger> : IAwaitableStateMachine<TState, TTrigger>
    {
        internal AwaitableStateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        internal InterlockedMonitor Monitor = new InterlockedMonitor();
        internal int isEnabled = 1;
        private readonly Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> configDictionary;

        internal AwaitableStateMachine(TState initialState,
            AwaitableStateMachineConfiguration<TState, TTrigger> configuration)
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

        public async Task MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            if (Monitor.TryEnter())
            {
                try
                {
                    if (!IsEnabled) return;
                    await MoveToStateInternal(state, option).ConfigureAwait(false);
                }
                finally
                {
                    Monitor.Exit();
                }
            }
            else
            {
                if (IsEnabled)
                    throw new InvalidOperationException(
                        "State cannot be changed while in transition. Use the AsyncStateMachine instead, if these semantics are required.");
            }
        }

        public async Task<bool> CanHandleTriggerAsync(TTrigger trigger)
        {
            foreach (var current in CurrentStateRepresentation.Triggers)
            {
                if (current.Trigger.Equals(trigger))
                {
                    if ((CheckFlag(current.TransitionFlags, AwaitableStateTransitionFlag.TriggerPredicateReturnsTask)))
                    {
                        var predicate = current.ConditionalTriggerPredicate as Func<Task<bool>>;
                        return predicate == null || await predicate();
                    }
                    else
                    {
                        var predicate = current.ConditionalTriggerPredicate as Func<bool>;
                        return predicate == null || predicate();
                    }
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

        public async Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (Monitor.TryEnter())
            {
                try
                {
                    if (!IsEnabled) return;
                    await FireInternalAsync(parameterizedTrigger, argument).ConfigureAwait(false);
                }
                finally
                {
                    Monitor.Exit();
                }
            }
            else
            {
                if (IsEnabled)
                    throw new InvalidOperationException(
                        "State cannot be changed while in transition. Use the AsyncStateMachine instead, if these semantics are required.");
            }
        }

        public async Task FireAsync(TTrigger trigger)
        {
            if (Monitor.TryEnter())
            {
                try
                {
                    if (!IsEnabled) return;
                    await FireInternalAsync(trigger).ConfigureAwait(false);
                }
                finally
                {
                    Monitor.Exit();
                }
            }
            else
            {
                if (IsEnabled)
                    throw new InvalidOperationException(
                        "State cannot be changed while in transition. Use the AsyncStateMachine instead, if these semantics are required.");
            }
        }

        public bool IsInTransition
        {
            get { return Monitor.IsBusy; }
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

        internal async Task MoveToStateInternal(TState state, StateTransitionOption option)
        {
            var currentRep = CurrentStateRepresentation;
            AwaitableStateRepresentation<TState, TTrigger> rep;
            if (configDictionary.TryGetValue(state, out rep))
            {
                if ((option & StateTransitionOption.CurrentStateExitTransition) ==
                    StateTransitionOption.CurrentStateExitTransition)
                {
                    if (CheckFlag(currentRep.TransitionFlags, AwaitableStateTransitionFlag.ExitReturnsTask))
                    {
                        var action = CurrentStateRepresentation.OnExitAction as Func<Task>;
                        if (action != null)
                            await action();
                    }
                    else
                    {
                        var action = CurrentStateRepresentation.OnExitAction as Action;
                        if (action != null)
                            action();
                    }
                }
                if ((option & StateTransitionOption.NewStateEntryTransition) ==
                    StateTransitionOption.NewStateEntryTransition)
                {
                    if (CheckFlag(rep.TransitionFlags, AwaitableStateTransitionFlag.EntryReturnsTask))
                    {
                        var action = rep.OnEntryAction as Func<Task>;
                        if (action != null)
                            await action();
                    }
                    else
                    {
                        var action = rep.OnEntryAction as Action;
                        if (action != null)
                            action();
                    }
                }

                CurrentStateRepresentation = rep;
            }
            else
            {
                throw new InvalidOperationException("Invalid state: " + state.ToString());
            }
        }

        internal async Task PerformStopTransitionAsync()
        {
            var current = CurrentStateRepresentation;
            if (CheckFlag(current.TransitionFlags, AwaitableStateTransitionFlag.ExitReturnsTask))
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

        internal async Task FireInternalAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            var trigger = parameterizedTrigger.Trigger;
            var triggerRep =
                AwaitableStateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    CurrentStateRepresentation);

            if (triggerRep == null)
            {
                HandleInvalidTrigger(trigger);
                return;
            }

            if (CheckFlag(triggerRep.TransitionFlags, AwaitableStateTransitionFlag.TriggerPredicateReturnsTask))
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
            if (CheckFlag(triggerRep.TransitionFlags, AwaitableStateTransitionFlag.TriggerActionReturnsTask))
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

            if (CheckFlag(CurrentStateRepresentation.TransitionFlags,
                AwaitableStateTransitionFlag.ExitReturnsTask))
            {
                var exit = (Func<Task>) CurrentStateRepresentation.OnExitAction;
                if (exit != null)
                    await exit();
            }
            else
            {
                var exit = (Action) CurrentStateRepresentation.OnExitAction;
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

            if (CheckFlag(nextStateRep.TransitionFlags, AwaitableStateTransitionFlag.EntryReturnsTask))
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

            var previousState = CurrentStateRepresentation.State;
            CurrentStateRepresentation = nextStateRep;

            // Raise event

            var sc = StateChanged;
            if (sc != null)
                sc(previousState, CurrentStateRepresentation.State);
        }

        internal async Task FireInternalAsync(TTrigger trigger)
        {
            var triggerRep =
                AwaitableStateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                    CurrentStateRepresentation);

            if (triggerRep == null)
            {
                HandleInvalidTrigger(trigger);
                return;
            }

            if (CheckFlag(triggerRep.TransitionFlags, AwaitableStateTransitionFlag.TriggerPredicateReturnsTask))
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
            if (CheckFlag(triggerRep.TransitionFlags, AwaitableStateTransitionFlag.TriggerActionReturnsTask))
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

            if (CheckFlag(CurrentStateRepresentation.TransitionFlags,
                AwaitableStateTransitionFlag.ExitReturnsTask))
            {
                var exit = (Func<Task>) CurrentStateRepresentation.OnExitAction;
                if (exit != null)
                    await exit();
            }
            else
            {
                var exit = (Action) CurrentStateRepresentation.OnExitAction;
                if (exit != null) exit();
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

            if (CheckFlag(nextStateRep.TransitionFlags, AwaitableStateTransitionFlag.EntryReturnsTask))
            {
                var entry = (Func<Task>) nextStateRep.OnEntryAction;
                if (entry != null)
                    await entry();
            }
            else
            {
                var entry = (Action) nextStateRep.OnEntryAction;
                if (entry != null) entry();
            }

            // Set states

            var previousState = CurrentStateRepresentation.State;
            CurrentStateRepresentation = nextStateRep;

            // Raise event

            var sc = StateChanged;
            if (sc != null) sc.Invoke(previousState, CurrentStateRepresentation.State);
        }

        private void HandleInvalidTrigger(TTrigger trigger)
        {
            var handler = UnhandledTriggerExecuted;
            if (handler != null) handler.Invoke(trigger, CurrentStateRepresentation.State);
        }

        private bool CheckFlag(AwaitableStateTransitionFlag source, AwaitableStateTransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }
    }
}
