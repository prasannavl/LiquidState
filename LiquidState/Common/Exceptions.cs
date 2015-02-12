// Author: Prasanna V. Loganathar
// Created: 12:14 AM 28-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace LiquidState.Common
{
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
            TExceptionTrigger trigger, TExceptionState state)
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