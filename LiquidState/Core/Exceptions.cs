// Author: Prasanna V. Loganathar
// Created: 12:14 AM 28-11-2014
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

        public static void Throw<TExceptionState>(TExceptionState state)
        {
            throw new InvalidStateException<TExceptionState>(state);
        }

        public static void Throw<TTrigger>(TransitionEventArgs<TState, TTrigger> eventArgs)
        {
            throw new InvalidStateException<TState>(eventArgs.TargetState);
        }
    }

    public class InvalidTriggerException<TTrigger, TState> : Exception
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

        public static void Throw<TExceptionTrigger, TExceptionState>(
            TriggerStateEventArgs<TExceptionState, TExceptionTrigger> eventArgs)
        {
            throw new InvalidTriggerException<TExceptionTrigger, TExceptionState>(eventArgs.Trigger,
                eventArgs.CurrentState);
        }

        public static void Throw<TExceptionTrigger, TExceptionState>(TExceptionTrigger trigger, TExceptionState state)
        {
            throw new InvalidTriggerException<TExceptionTrigger, TExceptionState>(trigger, state);
        }
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

        public static void Throw<TExceptionTrigger>(
            TExceptionTrigger trigger)
        {
            throw new InvalidTriggerParameterException<TExceptionTrigger>(trigger);
        }
    }
}
