// Author: Prasanna V. Loganathar
// Created: 10:58 PM 28-01-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Threading;

namespace LiquidState.Common
{
    public static class InterlockedHelpers
    {
        public static bool ExchangeIfGreaterThan(ref int location, int value, int comparand)
        {
            var initialValue = location;
            if (initialValue <= comparand) return false;
            if (Interlocked.CompareExchange(ref location, value, initialValue) == initialValue) return true;

            // Repeated to avoid spinwait allocation until necessary, even though its a struct
            var spinWait = new SpinWait();
            do
            {
                spinWait.SpinOnce();
                var currentValue = location;
                if (currentValue <= comparand) return false;
                if (Interlocked.CompareExchange(ref location, value, currentValue) == currentValue) return true;
            } while (true);
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

        public static void SpinWaitUntilCompareExchangeSucceeds<T>(ref T location, T value, T comparand) where T : class 
        {
            if (Interlocked.CompareExchange(ref location, value, comparand) == comparand) return;

            // Repeated to avoid spinwait allocation until necessary, even though its a struct
            var spinWait = new SpinWait();
            do
            {
                spinWait.SpinOnce();
            } while (Interlocked.CompareExchange(ref location, value, comparand) != comparand);
        }

        public static void SpinWaitUntilCompareExchangeSucceeds<T>(ref T location, Func<T> func, T comparand) where T : class
        {
            if (Interlocked.CompareExchange(ref location, func(), comparand) == comparand) return;

            // Repeated to avoid spinwait allocation until necessary, even though its a struct
            var spinWait = new SpinWait();
            do
            {
                spinWait.SpinOnce();
            } while (Interlocked.CompareExchange(ref location, func(), comparand) != comparand);
        }
    }

    public struct InterlockedMonitor
    {
        private int busy;

        public bool IsBusy
        {
            // Non critical to get latest value. So, try without impacting performance much with volatile.
            get { return Volatile.Read(ref busy) > 0; }
        }

        public void Enter()
        {
            InterlockedHelpers.SpinWaitUntilCompareExchangeSucceeds(ref busy, 1, 0);
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