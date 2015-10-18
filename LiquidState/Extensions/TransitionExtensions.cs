// Author: Prasanna V. Loganathar
// Created: 22:55 17-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using LiquidState.Core;

namespace LiquidState
{
    public static class TransitionExtensions
    {
        public static bool IsReentry<TState, TTrigger>(this Transition<TState, TTrigger> transition)
        {
            return transition.Source.Equals(transition.Destination);
        }
    }
}