using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Common;
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

        internal static bool CheckFlag(AwaitableTransitionFlag source, AwaitableTransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }

        internal static AwaitableStateRepresentation<TState, TTrigger> FindStateRepresentation<TState, TTrigger>(
            TState initialState, Dictionary<TState, AwaitableStateRepresentation<TState, TTrigger>> representations)
        {
            AwaitableStateRepresentation<TState, TTrigger> rep;
            return representations.TryGetValue(initialState, out rep) ? rep : null;
        }

        internal static async Task<DynamicState<TState>?> GetValidatedDynamicTransition<TState, TTrigger>(
            AwaitableTriggerRepresentation<TTrigger, TState> triggerRep)
        {
            DynamicState<TState> dynamicState;
            if (CheckFlag(triggerRep.AwaitableTransitionFlags, AwaitableTransitionFlag.DynamicStateReturnsTask))
            {
                dynamicState = await ((Func<Task<DynamicState<TState>>>) triggerRep.NextStateRepresentationWrapper)();
            }
            else
            {
                dynamicState = ((Func<DynamicState<TState>>) triggerRep.NextStateRepresentationWrapper)();
            }

            return dynamicState.CanTransition ? new DynamicState<TState>?(dynamicState) : null;
        }

        internal static async Task<bool> CanHandleTriggerAsync<TState, TTrigger>(TTrigger trigger,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, bool exactMatch = false)
        {
            var res = await FindAndEvaluateTriggerRepresentationAsync(trigger, machine, false);
            if (res == null) return false;

            if (!exactMatch) return true;

            if (CheckFlag(res.AwaitableTransitionFlags, AwaitableTransitionFlag.DynamicState))
            {
                if (await GetValidatedDynamicTransition(res) == null)
                    return false;
            }

            var currentType = res.OnTriggerAction.GetType();
            return CheckFlag(res.AwaitableTransitionFlags, AwaitableTransitionFlag.TriggerActionReturnsTask)
                ? currentType == typeof (Func<Task>)
                : currentType == typeof (Action);
        }

        internal static async Task<bool> CanHandleTriggerAsync<TState, TTrigger>(TTrigger trigger,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, Type argumentType)
        {
            var res = await FindAndEvaluateTriggerRepresentationAsync(trigger, machine, false);
            if (res == null) return false;

            if (CheckFlag(res.AwaitableTransitionFlags, AwaitableTransitionFlag.DynamicState))
            {
                if (await GetValidatedDynamicTransition(res) == null)
                    return false;
            }

            var currentType = res.OnTriggerAction.GetType();
            if (CheckFlag(res.AwaitableTransitionFlags, AwaitableTransitionFlag.TriggerActionReturnsTask))
            {
                var targetType = typeof (Func<>).MakeGenericType(argumentType, typeof (Task));
                return currentType == targetType;
            }
            else
            {
                var targetType = typeof (Action<>).MakeGenericType(argumentType);
                return currentType == targetType;
            }
        }

        internal static async Task<bool> CanHandleTriggerAsync<TState, TTrigger, TArgument>(TTrigger trigger,
            RawAwaitableStateMachineBase<TState, TTrigger> machine)
        {
            var res = await FindAndEvaluateTriggerRepresentationAsync(trigger, machine, false);
            if (res == null) return false;

            if (CheckFlag(res.AwaitableTransitionFlags, AwaitableTransitionFlag.DynamicState))
            {
                if (await GetValidatedDynamicTransition(res) == null)
                    return false;
            }

            var currentType = res.OnTriggerAction.GetType();
            if (CheckFlag(res.AwaitableTransitionFlags, AwaitableTransitionFlag.TriggerActionReturnsTask))
            {
                return currentType == typeof (Func<TArgument, Task>);
            }
            else
            {
                return currentType == typeof (Action<TArgument>);
            }
        }

        internal static async Task MoveToStateCoreAsync<TState, TTrigger>(TState state, StateTransitionOption option,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, bool raiseInvalidStates = true)
        {
            Contract.Requires(machine != null);

            AwaitableStateRepresentation<TState, TTrigger> targetRep;
            if (machine.Representations.TryGetValue(state, out targetRep))
            {
                var currentRep = machine.CurrentAwaitableStateRepresentation;
                machine.RaiseTransitionStarted(targetRep.State);

                if ((option & StateTransitionOption.CurrentStateExitTransition) ==
                    StateTransitionOption.CurrentStateExitTransition)
                {
                    if (CheckFlag(currentRep.AwaitableTransitionFlags, AwaitableTransitionFlag.ExitReturnsTask))
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
                    if (CheckFlag(targetRep.AwaitableTransitionFlags, AwaitableTransitionFlag.EntryReturnsTask))
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
                machine.CurrentAwaitableStateRepresentation = targetRep;
                machine.RaiseTransitionExecuted(pastState);
            }
            else
            {
                if (raiseInvalidStates)
                    machine.RaiseInvalidState(state);
            }
        }

        internal static async Task<AwaitableTriggerRepresentation<TTrigger, TState>> FindAndEvaluateTriggerRepresentationAsync
            <TState, TTrigger>(TTrigger trigger, RawAwaitableStateMachineBase<TState, TTrigger> machine,
                bool raiseInvalidTriggers = true)
        {
            Contract.Requires(machine != null);

            var triggerRep = AwaitableStateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                machine.CurrentAwaitableStateRepresentation);

            if (triggerRep == null)
            {
                if (raiseInvalidTriggers) machine.RaiseInvalidTrigger(trigger);
                return null;
            }

            if (CheckFlag(triggerRep.AwaitableTransitionFlags, AwaitableTransitionFlag.TriggerPredicateReturnsTask))
            {
                var predicate = (Func<Task<bool>>) triggerRep.ConditionalTriggerPredicate;
                if (predicate != null)
                    if (!await predicate())
                    {
                        if (raiseInvalidTriggers) machine.RaiseInvalidTrigger(trigger);
                        return null;
                    }
            }
            else
            {
                var predicate = (Func<bool>) triggerRep.ConditionalTriggerPredicate;
                if (predicate != null)
                    if (!predicate())
                    {
                        if (raiseInvalidTriggers) machine.RaiseInvalidTrigger(trigger);
                        return null;
                    }
            }

            // Handle ignored trigger

            if (triggerRep.NextStateRepresentationWrapper == null)
            {
                return null;
            }

            return triggerRep;
        }

        internal static async Task FireCoreAsync<TState, TTrigger>(TTrigger trigger,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, bool raiseInvalidStateOrTrigger = true)
        {
            Contract.Requires(machine != null);

            var currentStateRepresentation = machine.CurrentAwaitableStateRepresentation;
            var triggerRep =
                await FindAndEvaluateTriggerRepresentationAsync(trigger, machine, raiseInvalidStateOrTrigger);
            if (triggerRep == null)
                return;

            // Catch invalid paramters before execution.

            Action triggerAction = null;
            Func<Task> triggerFunc = null;
            if (CheckFlag(triggerRep.AwaitableTransitionFlags, AwaitableTransitionFlag.TriggerActionReturnsTask))
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

            if (CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicState))
            {
                var dynamicState = await GetValidatedDynamicTransition(triggerRep);
                if (dynamicState == null) return;

                var state = dynamicState.Value.ResultingState;

                nextAwaitableStateRep = FindStateRepresentation(state, machine.Representations);
                if (nextAwaitableStateRep == null)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidState(state);
                    return;
                }
            }
            else
            {
                nextAwaitableStateRep = (AwaitableStateRepresentation<TState, TTrigger>) triggerRep.NextStateRepresentationWrapper;
            }

            machine.RaiseTransitionStarted(nextAwaitableStateRep.State);

            // Current exit

            if (CheckFlag(currentStateRepresentation.AwaitableTransitionFlags,
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

            if (CheckFlag(nextAwaitableStateRep.AwaitableTransitionFlags, AwaitableTransitionFlag.EntryReturnsTask))
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
            machine.CurrentAwaitableStateRepresentation = nextAwaitableStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }

        internal static async Task FireCoreAsync<TState, TTrigger, TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, bool raiseInvalidStateOrTrigger = true)
        {
            Contract.Requires(machine != null);

            var currentStateRepresentation = machine.CurrentAwaitableStateRepresentation;
            var trigger = parameterizedTrigger.Trigger;

            var triggerRep =
                await FindAndEvaluateTriggerRepresentationAsync(trigger, machine, raiseInvalidStateOrTrigger);
            if (triggerRep == null)
                return;

            // Catch invalid parameters before execution.

            Action<TArgument> triggerAction = null;
            Func<TArgument, Task> triggerFunc = null;
            if (CheckFlag(triggerRep.AwaitableTransitionFlags, AwaitableTransitionFlag.TriggerActionReturnsTask))
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

            if (CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicState))
            {
                var dynamicState = await GetValidatedDynamicTransition(triggerRep);
                if (dynamicState == null) return;

                var state = dynamicState.Value.ResultingState;

                nextAwaitableStateRep = FindStateRepresentation(state, machine.Representations);
                if (nextAwaitableStateRep == null)
                {
                    if (raiseInvalidStateOrTrigger)
                        machine.RaiseInvalidState(state);
                    return;
                }
            }
            else
            {
                nextAwaitableStateRep = (AwaitableStateRepresentation<TState, TTrigger>) triggerRep.NextStateRepresentationWrapper;
            }

            machine.RaiseTransitionStarted(nextAwaitableStateRep.State);

            // Current exit

            if (CheckFlag(currentStateRepresentation.AwaitableTransitionFlags,
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

            if (CheckFlag(nextAwaitableStateRep.AwaitableTransitionFlags, AwaitableTransitionFlag.EntryReturnsTask))
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
            machine.CurrentAwaitableStateRepresentation = nextAwaitableStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }
    }
}
