using System;
using System.Diagnostics.Contracts;

namespace LiquidState.Configuration
{
    public class ParameterizedTrigger<TTrigger, TArgument>
    {
        public readonly Type ArgumentType = typeof(TArgument);
        public readonly TTrigger Trigger;

        internal ParameterizedTrigger(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Trigger = trigger;
        }
    }
}
