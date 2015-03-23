// Author: Prasanna V. Loganathar
// Created: 2:00 AM 12-02-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

namespace LiquidState.Common
{
    public enum StateTransitionOption : byte
    {
        SkipAllTransitions = 0,
        CurrentStateExitTransition = 1,
        NewStateEntryTransition = 2,
        Default = 3,
    }
}
