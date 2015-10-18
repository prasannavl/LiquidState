// Author: Prasanna V. Loganathar
// Created: 09:55 16-07-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using LiquidState.Awaitable.Core;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Awaitable
{
    public abstract class QueuedAwaitableStateMachineBase<TState, TTrigger> :
        RawAwaitableStateMachineBase<TState, TTrigger>
    {
        private IImmutableQueue<Func<Task>> actionsQueue;
        private int queueCount;
        private InterlockedBlockingMonitor queueMonitor = new InterlockedBlockingMonitor();
        private InterlockedMonitor monitor = new InterlockedMonitor();

        protected QueuedAwaitableStateMachineBase(TState initialState, AwaitableConfiguration<TState, TTrigger> config)
            : base(initialState, config)
        {
            actionsQueue = ImmutableQueue.Create<Func<Task>>();
        }

        public override async Task MoveToStateAsync(TState state,
            StateTransitionOption option = StateTransitionOption.Default)
        {
            var flag = true;

            queueMonitor.Enter();
            if (monitor.TryEnter())
            {
                if (queueCount == 0)
                {
                    queueMonitor.Exit();
                    flag = false;

                    try
                    {
                        await base.MoveToStateAsync(state, option);
                    }
                    finally
                    {
                        queueMonitor.Enter();
                        if (queueCount > 0)
                        {
                            queueMonitor.Exit();
                            var _ = StartQueueIfNecessaryAsync(true);
                            // Should not exit monitor here. Its handled by the process queue.
                        }
                        else
                        {
                            queueMonitor.Exit();
                            monitor.Exit();
                        }
                    }
                }
            }

            if (flag)
            {
                var tcs = new TaskCompletionSource<bool>();

                actionsQueue = actionsQueue.Enqueue(async () =>
                {
                    try
                    {
                        await base.MoveToStateAsync(state, option);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });

                queueCount++;
                queueMonitor.Exit();
                var _ = StartQueueIfNecessaryAsync();
                await tcs.Task;
            }
        }

        public override void Resume()
        {
            base.Resume();
            var _ = StartQueueIfNecessaryAsync();
        }

        public override async Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (!IsEnabled)
                return;

            var flag = true;

            queueMonitor.Enter();
            if (monitor.TryEnter())
            {
                // Try to execute inline if the process queue is empty.
                if (queueCount == 0)
                {
                    queueMonitor.Exit();
                    flag = false;

                    try
                    {
                        await base.FireAsync(parameterizedTrigger, argument);
                    }
                    finally
                    {
                        queueMonitor.Enter();
                        if (queueCount > 0)
                        {
                            queueMonitor.Exit();
                            var _ = StartQueueIfNecessaryAsync(true);
                            // Should not exit monitor here. Its handled by the process queue.
                        }
                        else
                        {
                            queueMonitor.Exit();
                            monitor.Exit();
                        }
                    }
                }
            }

            if (flag)
            {
                // Fast path was not taken. Queue up the delgates.

                var tcs = new TaskCompletionSource<bool>();
                actionsQueue = actionsQueue.Enqueue(async () =>
                {
                    try
                    {
                        await base.FireAsync(parameterizedTrigger, argument);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });
                queueCount++;
                queueMonitor.Exit();
                var _ = StartQueueIfNecessaryAsync();
                await tcs.Task;
            }
        }

        public override async Task FireAsync(TTrigger trigger)
        {
            if (!IsEnabled)
                return;

            var flag = true;

            queueMonitor.Enter();
            if (monitor.TryEnter())
            {
                // Try to execute inline if the process queue is empty.
                if (queueCount == 0)
                {
                    queueMonitor.Exit();
                    flag = false;

                    try
                    {
                        await base.FireAsync(trigger);
                    }
                    finally
                    {
                        queueMonitor.Enter();
                        if (queueCount > 0)
                        {
                            queueMonitor.Exit();
                            var _ = StartQueueIfNecessaryAsync(true);
                            // Should not exit monitor here. Its handled by the process queue.
                        }
                        else
                        {
                            queueMonitor.Exit();
                            monitor.Exit();
                        }
                    }
                }
            }


            if (flag)
            {
                // Fast path was not taken. Queue up the delgates.

                var tcs = new TaskCompletionSource<bool>();
                actionsQueue = actionsQueue.Enqueue(async () =>
                {
                    try
                    {
                        await base.FireAsync(trigger);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });
                queueCount++;
                queueMonitor.Exit();
                var _ = StartQueueIfNecessaryAsync();
                await tcs.Task;
            }
        }

        public void SkipPending()
        {
            queueMonitor.Enter();
            actionsQueue = ImmutableQueue<Func<Task>>.Empty;
            queueCount = 0;
            queueMonitor.Exit();
        }

        private Task StartQueueIfNecessaryAsync(bool lockTaken = false)
        {
            if (lockTaken || monitor.TryEnter())
            {
                return ProcessQueueInternal();
            }

            return TaskHelpers.CompletedTask;
        }

        private async Task ProcessQueueInternal()
        {
            // Always yield if the task was queued.
            await Task.Yield();

            queueMonitor.Enter();
            try
            {
                while (queueCount > 0)
                {
                    var current = actionsQueue.Peek();
                    actionsQueue = actionsQueue.Dequeue();
                    queueCount--;
                    queueMonitor.Exit();

                    try
                    {
                        if (current != null)
                            await current();
                    }
                    finally
                    {
                        queueMonitor.Enter();
                    }
                }
            }
            finally
            {
                // Exit monitor regardless of this method entering the monitor.
                monitor.Exit();
                queueMonitor.Exit();
            }
        }
    }

    public sealed class QueuedAwaitableStateMachine<TState, TTrigger> :
        QueuedAwaitableStateMachineBase<TState, TTrigger>
    {
        public QueuedAwaitableStateMachine(TState initialState,
            AwaitableConfiguration<TState, TTrigger> awaitableConfiguration)
            : base(initialState, awaitableConfiguration)
        {
        }
    }
}