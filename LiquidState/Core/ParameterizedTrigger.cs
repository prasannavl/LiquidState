// Author: Prasanna V. Loganathar
// Created: 04:18 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

namespace LiquidState.Core
{
    public class ParameterizedTrigger<TTrigger, TArgument>
    {
        public readonly TTrigger Trigger;

        internal ParameterizedTrigger(TTrigger trigger)
        {
            Trigger = trigger;
        }
    }
}