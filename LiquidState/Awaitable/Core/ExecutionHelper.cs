using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Awaitable.Core
{
    internal static class ExecutionHelper
    {
        internal static void ThrowInTransition()
        {
            throw new InvalidOperationException("State cannot be changed while in transition. Use an AsyncStateMachine if those are the semantics that's required.");
        }

        internal static bool CheckFlag(AwaitableStateTransitionFlag source, AwaitableStateTransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }

        internal static StateRepresentation<TState, TTrigger> FindStateRepresentation<TState, TTrigger>(TState initialState, Dictionary<TState, StateRepresentation<TState, TTrigger>> representations)
        {
            StateRepresentation<TState, TTrigger> rep;
            return representations.TryGetValue(initialState, out rep) ? rep : null;
        }

        internal static async Task MoveToStateCoreAsync<TState, TTrigger>(TState state, StateTransitionOption option, RawStateMachineBase<TState, TTrigger> machine)
        {

            Contract.Requires(machine != null);

            StateRepresentation<TState, TTrigger> targetRep;
            if (machine.Representations.TryGetValue(state, out targetRep))
            {

                var currentRep = machine.CurrentStateRepresentation;                
                machine.RaiseTransitionStarted(targetRep.State);

                if ((option & StateTransitionOption.CurrentStateExitTransition) ==
                                    StateTransitionOption.CurrentStateExitTransition)
                {
                    if (CheckFlag(currentRep.TransitionFlags, AwaitableStateTransitionFlag.ExitReturnsTask))
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
                    if (CheckFlag(targetRep.TransitionFlags, AwaitableStateTransitionFlag.EntryReturnsTask))
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
               machine.RaiseInvalidState(state);
            }
        }

        internal static async Task<TriggerRepresentation<TTrigger, TState>> FindAndEvaluateTriggerRepresentationAsync<TState, TTrigger>(TTrigger trigger, RawStateMachineBase<TState, TTrigger> machine)
        {
            Contract.Requires(machine != null);

            var triggerRep = StateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                machine.CurrentStateRepresentation);

            if (triggerRep == null)
            {
                machine.RaiseInvalidTrigger(trigger);
                return null;
            }

            if (CheckFlag(triggerRep.TransitionFlags, AwaitableStateTransitionFlag.TriggerPredicateReturnsTask))
            {
                var predicate = (Func<Task<bool>>)triggerRep.ConditionalTriggerPredicate;
                if (predicate != null)
                    if (!await predicate())
                    {
                        machine.RaiseInvalidTrigger(trigger);
                        return null;
                    }
            }
            else
            {
                var predicate = (Func<bool>)triggerRep.ConditionalTriggerPredicate;
                if (predicate != null)
                    if (!predicate())
                    {
                        machine.RaiseInvalidTrigger(trigger);
                        return null;
                    }
            }

            // Handle ignored trigger

            if (triggerRep.NextStateRepresentationPredicate == null)
            {
                return null;
            }

            return triggerRep;
        }

        internal static async Task FireCoreAsync<TState, TTrigger>(TTrigger trigger, RawStateMachineBase<TState, TTrigger> machine)
        {
            Contract.Requires(machine != null);

            var currentStateRepresentation = machine.CurrentStateRepresentation;
            var triggerRep = await FindAndEvaluateTriggerRepresentationAsync(trigger, machine);
            if (triggerRep == null)
                return;

            // Catch invalid paramters before execution.

            Action triggerAction = null;
            Func<Task> triggerFunc = null;
            if (CheckFlag(triggerRep.TransitionFlags, AwaitableStateTransitionFlag.TriggerActionReturnsTask))
            {
                try
                {
                    triggerFunc = (Func<Task>)triggerRep.OnTriggerAction;
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
                    triggerAction = (Action)triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                    return;
                }
            }

            StateRepresentation<TState, TTrigger> nextStateRep = null;

            if (CheckFlag(triggerRep.TransitionFlags,
                AwaitableStateTransitionFlag.NextStateRepresentationIsTargetStateTask))
            {
                var state = await ((Func<Task<TState>>)triggerRep.NextStateRepresentationPredicate)();
                nextStateRep = FindStateRepresentation(state, machine.Representations);
                if (nextStateRep == null)
                {
                    machine.RaiseInvalidState(state);
                    return;
                }
            }
            else
            {
                var func = (Func<StateRepresentation<TState, TTrigger>>)triggerRep.NextStateRepresentationPredicate;
                nextStateRep = func();

                // Simply return. Don't throw in this case, as it should be pre-evaluated during configuration.
                if (nextStateRep == null) return;
            }

            machine.RaiseTransitionStarted(nextStateRep.State);

            // Current exit

            if (CheckFlag(currentStateRepresentation.TransitionFlags,
                AwaitableStateTransitionFlag.ExitReturnsTask))
            {
                var exit = (Func<Task>)currentStateRepresentation.OnExitAction;
                if (exit != null)
                    await exit();
            }
            else
            {
                var exit = (Action)currentStateRepresentation.OnExitAction;
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

            var pastState = machine.CurrentState;
            machine.CurrentStateRepresentation = nextStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }

        internal static async Task FireCoreAsync<TState, TTrigger, TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument, RawStateMachineBase<TState, TTrigger> machine)
        {
            Contract.Requires(machine != null);

            var currentStateRepresentation = machine.CurrentStateRepresentation;
            var trigger = parameterizedTrigger.Trigger;

            var triggerRep = await FindAndEvaluateTriggerRepresentationAsync(trigger, machine);
            if (triggerRep == null)
                return;

            // Catch invalid parameters before execution.

            Action<TArgument> triggerAction = null;
            Func<TArgument, Task> triggerFunc = null;
            if (CheckFlag(triggerRep.TransitionFlags, AwaitableStateTransitionFlag.TriggerActionReturnsTask))
            {
                try
                {
                    triggerFunc = (Func<TArgument, Task>)triggerRep.OnTriggerAction;
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
                    triggerAction = (Action<TArgument>)triggerRep.OnTriggerAction;
                }
                catch (InvalidCastException)
                {
                    InvalidTriggerParameterException<TTrigger>.Throw(trigger);
                    return;
                }
            }

            StateRepresentation<TState, TTrigger> nextStateRep = null;

            if (CheckFlag(triggerRep.TransitionFlags,
                AwaitableStateTransitionFlag.NextStateRepresentationIsTargetStateTask))
            {
                var state = await ((Func<Task<TState>>)triggerRep.NextStateRepresentationPredicate)();
                nextStateRep = FindStateRepresentation(state, machine.Representations);
                if (nextStateRep == null)
                {
                    InvalidStateException<TState>.Throw(state);
                    return;
                }
            }
            else
            {
                var func = (Func<StateRepresentation<TState, TTrigger>>)triggerRep.NextStateRepresentationPredicate;
                nextStateRep = func();

                // Simply return. Don't throw in this case, as it should be pre-evaluated during configuration.
                if (nextStateRep == null) return;
            } 
            
            machine.RaiseTransitionStarted(nextStateRep.State);

            // Current exit

            if (CheckFlag(currentStateRepresentation.TransitionFlags,
                AwaitableStateTransitionFlag.ExitReturnsTask))
            {
                var exit = (Func<Task>)currentStateRepresentation.OnExitAction;
                if (exit != null)
                    await exit();
            }
            else
            {
                var exit = (Action)currentStateRepresentation.OnExitAction;
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

            if (CheckFlag(nextStateRep.TransitionFlags, AwaitableStateTransitionFlag.EntryReturnsTask))
            {
                var entry = (Func<Task>)nextStateRep.OnEntryAction;
                if (entry != null)
                    await entry();
            }
            else
            {
                var entry = (Action)nextStateRep.OnEntryAction;
                if (entry != null) entry();
            }

            var pastState = machine.CurrentState;
            machine.CurrentStateRepresentation = nextStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }
    }
}