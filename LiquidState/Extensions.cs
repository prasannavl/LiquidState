using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidState.Core;

namespace LiquidState
{
    public static class TranitionExtensions
    {
        public static bool IsReEntry<TState, TTrigger>(this Transition<TState, TTrigger> transition)
        {
            return transition.Source.Equals(transition.Destination);
        }
    }
}
