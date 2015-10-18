// Author: Prasanna V. Loganathar
// Created: 04:18 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

namespace LiquidState.Core
{
    public enum StateTransitionOption : byte
    {
        SkipAllTransitions = 0,
        CurrentStateExitTransition = 1,
        NewStateEntryTransition = 2,
        Default = 3,
    }
}