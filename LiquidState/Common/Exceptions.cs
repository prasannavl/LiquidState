using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidState.Common
{
    public class InvalidTriggerException<TTrigger, TState> : Exception
    {
        public TTrigger Trigger { get; private set; }
        public TState CurrentState { get; private set; }

        public InvalidTriggerException(TTrigger trigger, TState state) : base("Trigger is not allowed. Consider using Ignore in the configuration.")
        {
            Trigger = trigger;
            CurrentState = state;
        }

        public InvalidTriggerException(TTrigger trigger, TState state, string message) : base(message)
        {
            Trigger = trigger;
            CurrentState = state;
        }

        public static void ThrowInvalidTriggerException<TExceptionTrigger, TExceptionState>(
    TExceptionTrigger trigger, TExceptionState state)
        {
            throw new InvalidTriggerException<TExceptionTrigger, TExceptionState>(trigger, state);
        }
    }
}
