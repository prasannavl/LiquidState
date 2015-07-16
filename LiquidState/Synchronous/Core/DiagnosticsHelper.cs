using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidState.Core;

namespace LiquidState.Synchronous.Core
{
    internal static class DiagnosticsHelper
    {
        internal static IEnumerable<TTrigger> EnumeratePermittedTriggers<TState, TTrigger>(
            RawStateMachineBase<TState, TTrigger> machine)
        {
            foreach (var triggerRepresentation in machine.CurrentStateRepresentation.Triggers)
            {
                yield return triggerRepresentation.Trigger;
            }
        }

        internal static TriggerRepresentation<TTrigger, TState> FindAndEvaluateTriggerRepresentation<TState, TTrigger>(
            TTrigger trigger, RawStateMachineBase<TState, TTrigger> machine, bool raiseInvalidTriggers = true)
        {
            Contract.Requires(machine != null);

            var triggerRep = StateConfigurationHelper.FindTriggerRepresentation(trigger,
                machine.CurrentStateRepresentation);

            if (triggerRep == null)
            {
                if (raiseInvalidTriggers) machine.RaiseInvalidTrigger(trigger);
                return null;
            }


            var predicate = triggerRep.ConditionalTriggerPredicate;
            if (predicate != null)
            {
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

        internal static bool CanHandleTrigger<TState, TTrigger>(TTrigger trigger,
            RawStateMachineBase<TState, TTrigger> machine, bool exactMatch = false)
        {
            var res = FindAndEvaluateTriggerRepresentation(trigger, machine, false);
            if (res == null) return false;

            if (StateConfigurationHelper.CheckFlag(res.TransitionFlags,
                TransitionFlag.DynamicState))
            {
                var dynamicState = ((Func<DynamicState<TState>>) res.NextStateRepresentationWrapper)();
                if (!dynamicState.CanTransition)
                    return false;
            }

            if (!exactMatch) return true;
            return res.OnTriggerAction.GetType() == typeof(Action<Transition<TState, TTrigger>>);
        }

        internal static bool CanHandleTrigger<TState, TTrigger>(TTrigger trigger,
            RawStateMachineBase<TState, TTrigger> machine, Type argumentType)
        {
            var res = FindAndEvaluateTriggerRepresentation(trigger, machine, false);
            if (res == null) return false;

            if (StateConfigurationHelper.CheckFlag(res.TransitionFlags,
                TransitionFlag.DynamicState))
            {
                var dynamicState = ((Func<DynamicState<TState>>) res.NextStateRepresentationWrapper)();
                if (!dynamicState.CanTransition)
                    return false;
            }

            var type = typeof(Action<,>).MakeGenericType(typeof(Transition <TState, TTrigger>), argumentType);
            return res.OnTriggerAction.GetType() == type;
        }

        internal static bool CanHandleTrigger<TState, TTrigger, TArgument>(TTrigger trigger,
            RawStateMachineBase<TState, TTrigger> machine)
        {
            var res = FindAndEvaluateTriggerRepresentation(trigger, machine, false);
            if (res == null) return false;

            if (StateConfigurationHelper.CheckFlag(res.TransitionFlags,
                TransitionFlag.DynamicState))
            {
                var dynamicState = ((Func<DynamicState<TState>>) res.NextStateRepresentationWrapper)();
                if (!dynamicState.CanTransition)
                    return false;
            }

            return res.OnTriggerAction.GetType() == typeof (Action<Transition<TState, TTrigger>, TArgument>);
        }
    }
}
