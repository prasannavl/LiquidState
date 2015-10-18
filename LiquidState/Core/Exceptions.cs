// Author: Prasanna V. Loganathar
// Created: 04:18 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace LiquidState.Core
{
    public static class ExceptionHelper
    {
        internal static void ThrowExclusiveOperation()
        {
            throw new InvalidOperationException(
                "Permit* and Ignore* methods are exclusive to each other for a given resulting state.");
        }

        public static void ThrowInvalidState<TState>(TState state)
        {
            throw new InvalidStateException<TState>(state);
        }

        public static void ThrowInvalidState<TState, TTrigger>(TransitionEventArgs<TState, TTrigger> eventArgs)
        {
            throw new InvalidStateException<TState>(eventArgs.TargetState);
        }

        public static void ThrowInvalidTrigger<TState, TTrigger>(
            TriggerStateEventArgs<TState, TTrigger> eventArgs)
        {
            throw new InvalidTriggerException<TState, TTrigger>(eventArgs.Trigger,
                eventArgs.CurrentState);
        }

        public static void ThrowInvalidTrigger<TState, TTrigger>(TTrigger trigger, TState state)
        {
            throw new InvalidTriggerException<TState, TTrigger>(trigger, state);
        }

        public static void ThrowInvalidParameter<TTrigger>(
            TTrigger trigger)
        {
            throw new InvalidTriggerParameterException<TTrigger>(trigger);
        }
    }


    public class InvalidStateException<TState> : Exception
    {
        public InvalidStateException(TState state)
            : base("Invalid state: " + state.ToString())
        {
            InvalidState = state;
        }

        public InvalidStateException(TState state, string message)
            : base(message)
        {
            InvalidState = state;
        }

        public TState InvalidState { get; private set; }
    }

    public class InvalidTriggerException<TState, TTrigger> : Exception
    {
        public InvalidTriggerException(TTrigger trigger, TState state)
            : base("Trigger is not allowed. Consider using Ignore in the configuration.")
        {
            Trigger = trigger;
            CurrentState = state;
        }

        public InvalidTriggerException(TTrigger trigger, TState state, string message) : base(message)
        {
            Trigger = trigger;
            CurrentState = state;
        }

        public TTrigger Trigger { get; private set; }
        public TState CurrentState { get; private set; }
    }

    public class InvalidTriggerParameterException<TTrigger> : Exception
    {
        public InvalidTriggerParameterException(TTrigger trigger)
            : base(
                "Invalid trigger parameters. Appropriate ParamterizedTrigger has to be passed when, and only when the trigger is parameterized."
                )
        {
            Trigger = trigger;
        }

        public InvalidTriggerParameterException(TTrigger trigger, string message)
            : base(message)
        {
            Trigger = trigger;
        }

        public TTrigger Trigger { get; private set; }
    }
}