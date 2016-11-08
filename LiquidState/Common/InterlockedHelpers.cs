// Author: Prasanna V. Loganathar
// Created: 22:58 28-01-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Threading;

namespace LiquidState.Common
{
    internal static class InterlockedHelpers
    {
        public static void SpinWaitUntilCompareExchangeSucceeds(ref int location, int value, int comparand)
        {
            var spinWait = new SpinWait();
            while (Interlocked.CompareExchange(ref location, value, comparand) != comparand) spinWait.SpinOnce();
        }
    }

    /// <summary>
    ///     A struct is used for lesser over-head. But be very cautious about where its used.
    ///     And it should never be marked readonly, since the compiler will start reacting by creating copies
    ///     of mutation.
    /// </summary>
    internal struct InterlockedYieldableSpinMonitor
    {
        private int m_busy;
        public bool IsBusy => Interlocked.CompareExchange(ref m_busy, -1, -1) > 0;

        /// <summary>
        ///     WARNING: This method should NOT be used when there are awaits in-between Entry and Exit.
        ///     Task continuations expected to be run on the same thread (eg: UI context) will result in a deadlock.
        ///     This is also the reason why this functionality is not a part of the InterlockedMonitor itself, and is
        ///     isolated.
        /// </summary>
        public void Enter()
        {
            InterlockedHelpers.SpinWaitUntilCompareExchangeSucceeds(ref m_busy, 1, 0);
        }

        public void Exit()
        {
            Interlocked.Exchange(ref m_busy, 0);
        }
    }

    /// <summary>
    ///     A struct is used for lesser over-head. But be very cautious about where its used.
    ///     And it should never be marked readonly, since the compiler will start reacting by creating copies
    ///     for any inner mutation.
    /// </summary>
    internal struct InterlockedMonitor
    {
        private int m_busy;

        public bool IsBusy => Interlocked.CompareExchange(ref m_busy, -1, -1) > 0;

        public bool TryEnter()
        {
            return Interlocked.CompareExchange(ref m_busy, 1, 0) == 0;
        }

        public void Exit()
        {
            Interlocked.Exchange(ref m_busy, 0);
        }
    }
}