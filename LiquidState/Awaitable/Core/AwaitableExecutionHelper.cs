// Author: Prasanna V. Loganathar
// Created: 12:20 18-06-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Threading.Tasks;
using LiquidState.Core;

namespace LiquidState.Awaitable.Core
{
    internal static class AwaitableExecutionHelper
    {
        internal static void ThrowInTransition()
        {
            throw new InvalidOperationException(
                "State cannot be changed while already in transition. Tip: QueuedStateMachine has these parallel semantics which will work out of the box.");
        }

        internal static async Task MoveToStateCoreAsync<TState, TTrigger>(TState state, StateTransitionOption option,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, bool raiseInvalidStates = true)
        {
            AwaitableStateRepresentation<TState, TTrigger> targetRep;
            if (machine.Representations.TryGetValue(state, out targetRep))
            {
                var currentRep = machine.CurrentStateRepresentation;


                machine.RaiseTransitionStarted(targetRep.State);

                var transition = new Transition<TState, TTrigger>(currentRep.State, state);

                if ((option & StateTransitionOption.CurrentStateExitTransition) ==
                    StateTransitionOption.CurrentStateExitTransition)
                {
                    if (
                        AwaitableStateConfigurationHelper.CheckFlag(
                            currentRep.AwaitableTransitionFlags, AwaitableTransitionFlag.ExitReturnsTask))
                    {
                        var action = currentRep.OnExitAction as Func<Transition<TState, TTrigger>, Task>;
                        if (action != null)
                            await action(transition);
                    }
                    else
                    {
                        var action = currentRep.OnExitAction as Action<Transition<TState, TTrigger>>;
                        action?.Invoke(transition);
                    }
                }
                if ((option & StateTransitionOption.NewStateEntryTransition) ==
                    StateTransitionOption.NewStateEntryTransition)
                {
                    if (AwaitableStateConfigurationHelper.CheckFlag(
                        targetRep.AwaitableTransitionFlags, AwaitableTransitionFlag.EntryReturnsTask))
                    {
                        var action = targetRep.OnEntryAction as Func<Transition<TState, TTrigger>, Task>;
                        if (action != null)
                            await action(transition);
                    }
                    else
                    {
                        var action = targetRep.OnEntryAction as Action<Transition<TState, TTrigger>>;
                        action?.Invoke(transition);
                    }
                }

                var pastState = currentRep.State;
                machine.CurrentStateRepresentation = targetRep;
                machine.RaiseTransitionExecuted(pastState);
            }
            else
            {
                if (raiseInvalidStates)
                    machine.RaiseInvalidState(state);
            }
        }

        internal static async Task FireCoreAsync<TState, TTrigger>(TTrigger trigger,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, bool raiseInvalidStateOrTrigger = true)
        {
            var currentStateRepresentation = machine.CurrentStateRepresentation;


            var triggerRep =
                await
                    AwaitableDiagnosticsHelper.FindAndEvaluateTriggerRepresentationAsync(trigger, machine,
                        raiseInvalidStateOrTrigger);
            if (triggerRep == null)
                return;

            // Catch invalid paramters before execution.

            Action<Transition<TState, TTrigger>> triggerAction = null;
            Func<Transition<TState, TTrigger>, Task> triggerFunc = null;
            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.TriggerActionReturnsTask))
            {
                try
                {
                    triggerFunc = (Func<Transition<TState, TTrigger>, Task>)triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidTrigger(trigger);
                    return;
                }
            }
            else
            {
                try
                {
                    triggerAction = (Action<Transition<TState, TTrigger>>)triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidTrigger(trigger);
                    return;
                }
            }

            AwaitableStateRepresentation<TState, TTrigger> nextStateRep = null;

            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicState))
            {
                var dynamicState = await AwaitableDiagnosticsHelper.GetValidatedDynamicTransition<TState, TTrigger>(triggerRep);
                if (dynamicState == null) return;

                var state = dynamicState.Value.ResultingState;

                nextStateRep =
                    AwaitableStateConfigurationHelper.FindStateRepresentation(state,
                        machine.Representations);
                if (nextStateRep == null)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidState(state);
                    return;
                }
            }
            else
            {
                nextStateRep =
                    (AwaitableStateRepresentation<TState, TTrigger>)triggerRep.NextStateRepresentationWrapper;
            }


            var transition = new Transition<TState, TTrigger>(currentStateRepresentation.State, nextStateRep.State);

            machine.RaiseTransitionStarted(nextStateRep.State);

            // Current exit

            if (
                AwaitableStateConfigurationHelper.CheckFlag(
                    currentStateRepresentation.AwaitableTransitionFlags,
                    AwaitableTransitionFlag.ExitReturnsTask))
            {
                var exit = (Func<Transition<TState, TTrigger>, Task>)currentStateRepresentation.OnExitAction;
                if (exit != null)
                    await exit(transition);
            }
            else
            {
                var exit = (Action<Transition<TState, TTrigger>>)currentStateRepresentation.OnExitAction;
                exit?.Invoke(transition);
            }

            // Trigger entry

            if (triggerAction != null)
            {
                triggerAction(transition);
            }
            else if (triggerFunc != null)
            {
                await triggerFunc(transition);
            }

            // Next entry

            if (
                AwaitableStateConfigurationHelper.CheckFlag(
                    nextStateRep.AwaitableTransitionFlags, AwaitableTransitionFlag.EntryReturnsTask))
            {
                var entry = (Func<Transition<TState, TTrigger>, Task>)nextStateRep.OnEntryAction;
                if (entry != null)
                    await entry(transition);
            }
            else
            {
                var entry = (Action<Transition<TState, TTrigger>>)nextStateRep.OnEntryAction;
                entry?.Invoke(transition);
            }

            var pastState = machine.CurrentState;
            machine.CurrentStateRepresentation = nextStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }

        internal static async Task FireCoreAsync<TState, TTrigger, TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, bool raiseInvalidStateOrTrigger = true)
        {
            var currentStateRepresentation = machine.CurrentStateRepresentation;


            var trigger = parameterizedTrigger.Trigger;

            var triggerRep =
                await
                    AwaitableDiagnosticsHelper.FindAndEvaluateTriggerRepresentationAsync(trigger, machine,
                        raiseInvalidStateOrTrigger);
            if (triggerRep == null)
                return;

            // Catch invalid parameters before execution.

            Action<Transition<TState, TTrigger>, TArgument> triggerAction = null;
            Func<Transition<TState, TTrigger>, TArgument, Task> triggerFunc = null;
            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.TriggerActionReturnsTask))
            {
                try
                {
                    triggerFunc = (Func<Transition<TState, TTrigger>, TArgument, Task>)triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidTrigger(trigger);
                    return;
                }
            }
            else
            {
                try
                {
                    triggerAction = (Action<Transition<TState, TTrigger>, TArgument>)triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidTrigger(trigger);
                    return;
                }
            }

            AwaitableStateRepresentation<TState, TTrigger> nextStateRep = null;

            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicState))
            {
                var dynamicState = await AwaitableDiagnosticsHelper.GetValidatedDynamicTransition<TState, TTrigger>(triggerRep);
                if (dynamicState == null) return;

                var state = dynamicState.Value.ResultingState;

                nextStateRep =
                    AwaitableStateConfigurationHelper.FindStateRepresentation(state,
                        machine.Representations);
                if (nextStateRep == null)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidState(state);
                    return;
                }
            }
            else
            {
                nextStateRep =
                    (AwaitableStateRepresentation<TState, TTrigger>)triggerRep.NextStateRepresentationWrapper;
            }


            var transition = new Transition<TState, TTrigger>(currentStateRepresentation.State, nextStateRep.State);

            machine.RaiseTransitionStarted(nextStateRep.State);

            // Current exit

            if (
                AwaitableStateConfigurationHelper.CheckFlag(
                    currentStateRepresentation.AwaitableTransitionFlags,
                    AwaitableTransitionFlag.ExitReturnsTask))
            {
                var exit = (Func<Transition<TState, TTrigger>, Task>)currentStateRepresentation.OnExitAction;
                if (exit != null)
                    await exit(transition);
            }
            else
            {
                var exit = (Action<Transition<TState, TTrigger>>)currentStateRepresentation.OnExitAction;
                exit?.Invoke(transition);
            }

            // Trigger entry

            if (triggerAction != null)
            {
                triggerAction(transition, argument);
            }
            else if (triggerFunc != null)
            {
                await triggerFunc(transition, argument);
            }

            // Next entry

            if (
                AwaitableStateConfigurationHelper.CheckFlag(
                    nextStateRep.AwaitableTransitionFlags, AwaitableTransitionFlag.EntryReturnsTask))
            {
                var entry = (Func<Transition<TState, TTrigger>, Task>)nextStateRep.OnEntryAction;
                if (entry != null)
                    await entry(transition);
            }
            else
            {
                var entry = (Action<Transition<TState, TTrigger>>)nextStateRep.OnEntryAction;
                entry?.Invoke(transition);
            }

            var pastState = machine.CurrentState;
            machine.CurrentStateRepresentation = nextStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }
    }
}