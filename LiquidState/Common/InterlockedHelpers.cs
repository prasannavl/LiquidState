// Author: Prasanna V. Loganathar
// Created: 10:58 PM 28-01-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Reflection;
using System.Threading;

namespace LiquidState.Common
{
    internal static class InterlockedHelpers
    {
        public static void HybridSleepSpinUntilCompareExchangeSucceeds(ref int location, int value, int comparand)
        {
            if (Interlocked.CompareExchange(ref location, value, comparand) == comparand) return;

            double waitTime = 20;
            // Repeated to avoid spinwait allocation until necessary, even though its a struct
            var spinWait = new SpinWait();
            do
            {
                if (spinWait.NextSpinWillYield)
                {
                    new ManualResetEvent(false).WaitOne((int) waitTime);
                    if (waitTime <= 100)
                        waitTime = waitTime*1.3;
                }
                else
                {
                    spinWait.SpinOnce();
                }
            } while (Interlocked.CompareExchange(ref location, value, comparand) != comparand);
        }

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

        public static void SpinWaitUntilCompareExchangeSucceeds(ref int location, int value, int comparand,
            string message)
        {
            if (Interlocked.CompareExchange(ref location, value, comparand) == comparand) return;

            // Repeated to avoid spinwait allocation until necessary, even though its a struct
            var spinWait = new SpinWait();
            do
            {
                if (spinWait.NextSpinWillYield)
                {
                    var type = Type.GetType("System.Console").GetRuntimeMethod("WriteLine", new[] {typeof (string)});
                    type.Invoke(null, new[] {message});
                }
                spinWait.SpinOnce();
            } while (Interlocked.CompareExchange(ref location, value, comparand) != comparand);
        }
    }

    internal struct InterlockedMonitor
    {
        private int busy;

        public bool IsBusy
        {
            get { return Interlocked.CompareExchange(ref busy, -1, -1) > 0; }
        }

        public void Enter()
        {
            InterlockedHelpers.SpinWaitUntilCompareExchangeSucceeds(ref busy, 1, 0);
        }

        public void EnterWithDebugLogging(string message)
        {
#if DEBUG
            InterlockedHelpers.SpinWaitUntilCompareExchangeSucceeds(ref busy, 1, 0, message);
#else
            Enter();
#endif
        }

        public void EnterWithHybridSpin()
        {
            InterlockedHelpers.HybridSleepSpinUntilCompareExchangeSucceeds(ref busy, 1, 0);
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