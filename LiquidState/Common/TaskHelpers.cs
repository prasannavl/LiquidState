// Author: Prasanna V. Loganathar
// Created: 3:49 PM 07-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Threading.Tasks;

namespace LiquidState.Common
{
    internal static class TaskHelpers
    {
        public static Task CompletedTask = Task.FromResult(false);

        public static Task PropagateContinuationTo(this Task task, TaskCompletionSource<bool> tcs)
        {
            return task
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        tcs.TrySetCanceled();
                    else if (t.IsFaulted)
                        tcs.SetException(t.Exception);
                    else
                        tcs.TrySetResult(true);
                });
        }

        public static Task PropagateContinuationTo<TResult>(this Task<TResult> task, TaskCompletionSource<TResult> tcs)
        {
            return task
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        tcs.TrySetCanceled();
                    else if (t.IsFaulted)
                        tcs.SetException(t.Exception);
                    else
                        tcs.TrySetResult(t.Result);
                });
        }
    }
}