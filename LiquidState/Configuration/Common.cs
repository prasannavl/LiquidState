// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.Contracts;

namespace LiquidState.Configuration
{
    public class ParameterizedTrigger<TTrigger, TArgument>
    {
        public readonly Type ArgumentType = typeof (TArgument);
        public readonly TTrigger Trigger;

        internal ParameterizedTrigger(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Trigger = trigger;
        }
    }
}