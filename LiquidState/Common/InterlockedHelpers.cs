// Author: Prasanna V. Loganathar
// Created: 10:58 PM 28-01-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Threading;

namespace LiquidState.Common
{
    internal static class InterlockedHelpers
    {
        public static void SpinWaitUntilCompareExchangeSucceeds(ref int location, int value, int comparand)
        {
            if (Interlocked.CompareExchange(ref location, value, comparand) == comparand) return;

            // Repeated to avoid spinwait allocation until necessary, even though its a struct
            var spinWait = new SpinWait();
            do
            {
                spinWait.SpinOnce();
            } while (Interlocked.CompareExchange(ref location, value, comparand) != comparand);
        }
    }

    internal struct InterlockedBlockingMonitor
    {
        private int busy;

        public bool IsBusy
        {
            get { return Interlocked.CompareExchange(ref busy, -1, -1) > 0; }
        }

        /// <summary>
        ///     WARNING: This method has to be not be used with awaits in-between Entry and Exit.
        ///     Task continuations expected to be run on the same thread (eg: UI context) will result in a deadlock.
        ///     This is also the reason why this functionality is not a part of the InterlockedMonitor itself, and is
        ///     isolated.
        /// </summary>
        public void Enter()
        {
            InterlockedHelpers.SpinWaitUntilCompareExchangeSucceeds(ref busy, 1, 0);
        }

        public void Exit()
        {
            Interlocked.Exchange(ref busy, 0);
        }
    }

    internal struct InterlockedMonitor
    {
        private int busy;

        public bool IsBusy
        {
            get { return Interlocked.CompareExchange(ref busy, -1, -1) > 0; }
        }

        public bool TryEnter()
        {
            return Interlocked.CompareExchange(ref busy, 1, 0) == 0;
        }

        public void Exit()
        {
            Interlocked.Exchange(ref busy, 0);
        }
    }
}