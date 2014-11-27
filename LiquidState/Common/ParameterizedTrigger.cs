using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidState.Common
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
