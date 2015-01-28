// Author: Prasanna V. Loganathar
// Created: 3:41 PM 07-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Threading.Tasks;

namespace LiquidState.Common
{
    public interface IDispatcher
    {
        TaskScheduler Scheduler { get; }
        void Initialize();
        bool CheckAccess();
        void Execute(Action action);
        void Execute<T>(Action<T> action, T state);
    }
}