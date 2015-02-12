// Author: Prasanna V. Loganathar
// Created: 1:09 AM 28-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics.Contracts;

namespace LiquidState.Common
{
    public class ParameterizedTrigger<TTrigger, TArgument>
    {
        public readonly TTrigger Trigger;

        internal ParameterizedTrigger(TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            Trigger = trigger;
        }
    }
}