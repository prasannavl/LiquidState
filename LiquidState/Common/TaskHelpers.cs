// Author: Prasanna V. Loganathar
// Created: 3:41 PM 07-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiquidState.Common
{
    public static class TaskHelpers
    {
        public static Task ExecuteOnSchedulerAsync(TaskScheduler scheduler, Func<Task> task)
        {
            return Task.Factory.StartNew(task,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                scheduler).Unwrap();
        }
    }
}