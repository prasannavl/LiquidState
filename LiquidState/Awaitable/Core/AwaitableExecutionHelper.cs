using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires(machine != null);

            AwaitableStateRepresentation<TState, TTrigger> targetRep;
            if (machine.Representations.TryGetValue(state, out targetRep))
            {
                var currentRep = machine.CurrentStateRepresentation;
                machine.RaiseTransitionStarted(targetRep.State);

                if ((option & StateTransitionOption.CurrentStateExitTransition) ==
                    StateTransitionOption.CurrentStateExitTransition)
                {
                    if (
                        AwaitableStateConfigurationHelper.CheckFlag(
                            currentRep.AwaitableTransitionFlags, AwaitableTransitionFlag.ExitReturnsTask))
                    {
                        var action = currentRep.OnExitAction as Func<Task>;
                        if (action != null)
                            await action();
                    }
                    else
                    {
                        var action = currentRep.OnExitAction as Action;
                        if (action != null)
                            action();
                    }
                }
                if ((option & StateTransitionOption.NewStateEntryTransition) ==
                    StateTransitionOption.NewStateEntryTransition)
                {
                    if (AwaitableStateConfigurationHelper.CheckFlag(
                        targetRep.AwaitableTransitionFlags, AwaitableTransitionFlag.EntryReturnsTask))
                    {
                        var action = targetRep.OnEntryAction as Func<Task>;
                        if (action != null)
                            await action();
                    }
                    else
                    {
                        var action = targetRep.OnEntryAction as Action;
                        if (action != null)
                            action();
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
            Contract.Requires(machine != null);

            var currentStateRepresentation = machine.CurrentStateRepresentation;
            var triggerRep =
                await
                    AwaitableDiagnosticsHelper.FindAndEvaluateTriggerRepresentationAsync(trigger, machine,
                        raiseInvalidStateOrTrigger);
            if (triggerRep == null)
                return;

            // Catch invalid paramters before execution.

            Action triggerAction = null;
            Func<Task> triggerFunc = null;
            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.TriggerActionReturnsTask))
            {
                try
                {
                    triggerFunc = (Func<Task>) triggerRep.OnTriggerAction;
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
                    triggerAction = (Action) triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidTrigger(trigger);
                    return;
                }
            }

            AwaitableStateRepresentation<TState, TTrigger> nextAwaitableStateRep = null;

            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicState))
            {
                var dynamicState = await AwaitableDiagnosticsHelper.GetValidatedDynamicTransition(triggerRep);
                if (dynamicState == null) return;

                var state = dynamicState.Value.ResultingState;

                nextAwaitableStateRep =
                    AwaitableStateConfigurationHelper.FindStateRepresentation(state,
                        machine.Representations);
                if (nextAwaitableStateRep == null)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidState(state);
                    return;
                }
            }
            else
            {
                nextAwaitableStateRep =
                    (AwaitableStateRepresentation<TState, TTrigger>) triggerRep.NextStateRepresentationWrapper;
            }

            machine.RaiseTransitionStarted(nextAwaitableStateRep.State);

            // Current exit

            if (
                AwaitableStateConfigurationHelper.CheckFlag(
                    currentStateRepresentation.AwaitableTransitionFlags,
                    AwaitableTransitionFlag.ExitReturnsTask))
            {
                var exit = (Func<Task>) currentStateRepresentation.OnExitAction;
                if (exit != null)
                    await exit();
            }
            else
            {
                var exit = (Action) currentStateRepresentation.OnExitAction;
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

            // Next entry

            if (
                AwaitableStateConfigurationHelper.CheckFlag(
                    nextAwaitableStateRep.AwaitableTransitionFlags, AwaitableTransitionFlag.EntryReturnsTask))
            {
                var entry = (Func<Task>) nextAwaitableStateRep.OnEntryAction;
                if (entry != null)
                    await entry();
            }
            else
            {
                var entry = (Action) nextAwaitableStateRep.OnEntryAction;
                if (entry != null) entry();
            }

            var pastState = machine.CurrentState;
            machine.CurrentStateRepresentation = nextAwaitableStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }

        internal static async Task FireCoreAsync<TState, TTrigger, TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, bool raiseInvalidStateOrTrigger = true)
        {
            Contract.Requires(machine != null);

            var currentStateRepresentation = machine.CurrentStateRepresentation;
            var trigger = parameterizedTrigger.Trigger;

            var triggerRep =
                await
                    AwaitableDiagnosticsHelper.FindAndEvaluateTriggerRepresentationAsync(trigger, machine,
                        raiseInvalidStateOrTrigger);
            if (triggerRep == null)
                return;

            // Catch invalid parameters before execution.

            Action<TArgument> triggerAction = null;
            Func<TArgument, Task> triggerFunc = null;
            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.TriggerActionReturnsTask))
            {
                try
                {
                    triggerFunc = (Func<TArgument, Task>) triggerRep.OnTriggerAction;
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
                    triggerAction = (Action<TArgument>) triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidTrigger(trigger);
                    return;
                }
            }

            AwaitableStateRepresentation<TState, TTrigger> nextAwaitableStateRep = null;

            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicState))
            {
                var dynamicState = await AwaitableDiagnosticsHelper.GetValidatedDynamicTransition(triggerRep);
                if (dynamicState == null) return;

                var state = dynamicState.Value.ResultingState;

                nextAwaitableStateRep =
                    AwaitableStateConfigurationHelper.FindStateRepresentation(state,
                        machine.Representations);
                if (nextAwaitableStateRep == null)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidState(state);
                    return;
                }
            }
            else
            {
                nextAwaitableStateRep =
                    (AwaitableStateRepresentation<TState, TTrigger>) triggerRep.NextStateRepresentationWrapper;
            }

            machine.RaiseTransitionStarted(nextAwaitableStateRep.State);

            // Current exit

            if (
                AwaitableStateConfigurationHelper.CheckFlag(
                    currentStateRepresentation.AwaitableTransitionFlags,
                    AwaitableTransitionFlag.ExitReturnsTask))
            {
                var exit = (Func<Task>) currentStateRepresentation.OnExitAction;
                if (exit != null)
                    await exit();
            }
            else
            {
                var exit = (Action) currentStateRepresentation.OnExitAction;
                if (exit != null) exit();
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

            // Next entry

            if (
                AwaitableStateConfigurationHelper.CheckFlag(
                    nextAwaitableStateRep.AwaitableTransitionFlags, AwaitableTransitionFlag.EntryReturnsTask))
            {
                var entry = (Func<Task>) nextAwaitableStateRep.OnEntryAction;
                if (entry != null)
                    await entry();
            }
            else
            {
                var entry = (Action) nextAwaitableStateRep.OnEntryAction;
                if (entry != null) entry();
            }

            var pastState = machine.CurrentState;
            machine.CurrentStateRepresentation = nextAwaitableStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }
    }
}
