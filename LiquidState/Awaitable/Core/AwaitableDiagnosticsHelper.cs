// Author: Prasanna V. Loganathar
// Created: 12:06 18-06-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiquidState.Awaitable.Core
{
    internal static class AwaitableDiagnosticsHelper
    {
        internal static IEnumerable<TTrigger> EnumeratePermittedTriggers<TState, TTrigger>(
            RawAwaitableStateMachineBase<TState, TTrigger> machine)
        {
            foreach (var triggerRepresentation in machine.CurrentStateRepresentation.Triggers) {
                yield return triggerRepresentation.Trigger;
            }
        }

        internal static async Task<DynamicState<TState>?> GetValidatedDynamicTransition<TState, TTrigger>(
            AwaitableTriggerRepresentation<TTrigger> triggerRep)
        {
            DynamicState<TState> dynamicState;
            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicStateReturnsTask)) {
                dynamicState = await ((Func<Task<DynamicState<TState>>>) triggerRep.NextStateRepresentationWrapper)();
            }
            else
            {
                dynamicState = ((Func<DynamicState<TState>>) triggerRep.NextStateRepresentationWrapper)();
            }

            return dynamicState.CanTransition ? new DynamicState<TState>?(dynamicState) : null;
        }

        internal static async Task<AwaitableTriggerRepresentation<TTrigger>>
            FindAndEvaluateTriggerRepresentationAsync
            <TState, TTrigger>(TTrigger trigger, RawAwaitableStateMachineBase<TState, TTrigger> machine,
                bool raiseInvalidTriggers = true)
        {
            var triggerRep = AwaitableStateConfigurationHelper.FindTriggerRepresentation(trigger,
                machine.CurrentStateRepresentation);

            if (triggerRep == null)
            {
                if (raiseInvalidTriggers) machine.RaiseInvalidTrigger(trigger);
                return null;
            }

            if (AwaitableStateConfigurationHelper.CheckFlag(triggerRep.AwaitableTransitionFlags,
                AwaitableTransitionFlag.TriggerPredicateReturnsTask))
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

            if (triggerRep.NextStateRepresentationWrapper == null) { return null; }

            return triggerRep;
        }

        internal static async Task<bool> CanHandleTriggerAsync<TState, TTrigger>(TTrigger trigger,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, bool exactMatch = false)
        {
            var res = await FindAndEvaluateTriggerRepresentationAsync(trigger, machine, false);
            if (res == null) return false;

            if (!exactMatch) return true;

            if (AwaitableStateConfigurationHelper.CheckFlag(res.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicState))
            {
                if (await GetValidatedDynamicTransition<TState, TTrigger>(res) == null) return false;
            }

            var currentType = res.OnTriggerAction.GetType();
            return AwaitableStateConfigurationHelper.CheckFlag(res.AwaitableTransitionFlags,
                AwaitableTransitionFlag.TriggerActionReturnsTask)
                ? currentType == typeof(Func<Task>)
                : currentType == typeof(Action);
        }

        internal static async Task<bool> CanHandleTriggerAsync<TState, TTrigger>(TTrigger trigger,
            RawAwaitableStateMachineBase<TState, TTrigger> machine, Type argumentType)
        {
            var res = await FindAndEvaluateTriggerRepresentationAsync(trigger, machine, false);
            if (res == null) return false;

            if (AwaitableStateConfigurationHelper.CheckFlag(res.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicState))
            {
                if (await GetValidatedDynamicTransition<TState, TTrigger>(res) == null) return false;
            }

            var currentType = res.OnTriggerAction.GetType();
            if (AwaitableStateConfigurationHelper.CheckFlag(res.AwaitableTransitionFlags,
                AwaitableTransitionFlag.TriggerActionReturnsTask))
            {
                var targetType = typeof(Func<>).MakeGenericType(argumentType, typeof(Task));
                return currentType == targetType;
            }
            else
            {
                var targetType = typeof(Action<>).MakeGenericType(argumentType);
                return currentType == targetType;
            }
        }

        internal static async Task<bool> CanHandleTriggerAsync<TState, TTrigger, TArgument>(TTrigger trigger,
            RawAwaitableStateMachineBase<TState, TTrigger> machine)
        {
            var res = await FindAndEvaluateTriggerRepresentationAsync(trigger, machine, false);
            if (res == null) return false;

            if (AwaitableStateConfigurationHelper.CheckFlag(res.AwaitableTransitionFlags,
                AwaitableTransitionFlag.DynamicState))
            {
                if (await GetValidatedDynamicTransition<TState, TTrigger>(res) == null) return false;
            }

            var currentType = res.OnTriggerAction.GetType();
            if (AwaitableStateConfigurationHelper.CheckFlag(res.AwaitableTransitionFlags,
                AwaitableTransitionFlag.TriggerActionReturnsTask)) {
                return currentType == typeof(Func<TArgument, Task>);
            }
            return currentType == typeof(Action<TArgument>);
        }
    }
}