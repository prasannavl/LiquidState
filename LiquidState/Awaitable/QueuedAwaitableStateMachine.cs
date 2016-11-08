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
        private IImmutableQueue<Func<Task>> m_actionsQueue;
        private InterlockedMonitor m_monitor = new InterlockedMonitor();
        private int m_queueCount;
        private InterlockedYieldableSpinMonitor m_queueMonitor = new InterlockedYieldableSpinMonitor();

        protected QueuedAwaitableStateMachineBase(TState initialState, AwaitableConfiguration<TState, TTrigger> config)
            : base(initialState, config)
        {
            m_actionsQueue = ImmutableQueue.Create<Func<Task>>();
        }

        public override async Task MoveToStateAsync(TState state,
            StateTransitionOption option = StateTransitionOption.Default)
        {
            var flag = true;

            m_queueMonitor.Enter();
            if (m_monitor.TryEnter())
            {
                if (m_queueCount == 0)
                {
                    m_queueMonitor.Exit();
                    flag = false;

                    try { await base.MoveToStateAsync(state, option); }
                    finally
                    {
                        m_queueMonitor.Enter();
                        if (m_queueCount > 0)
                        {
                            m_queueMonitor.Exit();
                            var _ = StartQueueIfNecessaryAsync(true);
                            // Should not exit monitor here. Its handled by the process queue.
                        }
                        else
                        {
                            m_queueMonitor.Exit();
                            m_monitor.Exit();
                        }
                    }
                }
            }

            if (flag)
            {
                var tcs = new TaskCompletionSource<bool>();

                m_actionsQueue = m_actionsQueue.Enqueue(async () =>
                {
                    try
                    {
                        await base.MoveToStateAsync(state, option);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex) {
                        tcs.SetException(ex);
                    }
                });

                m_queueCount++;
                m_queueMonitor.Exit();
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
            if (!IsEnabled) return;

            var flag = true;

            m_queueMonitor.Enter();
            if (m_monitor.TryEnter())
            {
                // Try to execute inline if the process queue is empty.
                if (m_queueCount == 0)
                {
                    m_queueMonitor.Exit();
                    flag = false;

                    try { await base.FireAsync(parameterizedTrigger, argument); }
                    finally
                    {
                        m_queueMonitor.Enter();
                        if (m_queueCount > 0)
                        {
                            m_queueMonitor.Exit();
                            var _ = StartQueueIfNecessaryAsync(true);
                            // Should not exit monitor here. Its handled by the process queue.
                        }
                        else
                        {
                            m_queueMonitor.Exit();
                            m_monitor.Exit();
                        }
                    }
                }
            }

            if (flag)
            {
                // Fast path was not taken. Queue up the delgates.

                var tcs = new TaskCompletionSource<bool>();
                m_actionsQueue = m_actionsQueue.Enqueue(async () =>
                {
                    try
                    {
                        await base.FireAsync(parameterizedTrigger, argument);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex) {
                        tcs.TrySetException(ex);
                    }
                });
                m_queueCount++;
                m_queueMonitor.Exit();
                var _ = StartQueueIfNecessaryAsync();
                await tcs.Task;
            }
        }

        public override async Task FireAsync(TTrigger trigger)
        {
            if (!IsEnabled) return;

            var flag = true;

            m_queueMonitor.Enter();
            if (m_monitor.TryEnter())
            {
                // Try to execute inline if the process queue is empty.
                if (m_queueCount == 0)
                {
                    m_queueMonitor.Exit();
                    flag = false;

                    try { await base.FireAsync(trigger); }
                    finally
                    {
                        m_queueMonitor.Enter();
                        if (m_queueCount > 0)
                        {
                            m_queueMonitor.Exit();
                            var _ = StartQueueIfNecessaryAsync(true);
                            // Should not exit monitor here. Its handled by the process queue.
                        }
                        else
                        {
                            m_queueMonitor.Exit();
                            m_monitor.Exit();
                        }
                    }
                }
            }


            if (flag)
            {
                // Fast path was not taken. Queue up the delgates.

                var tcs = new TaskCompletionSource<bool>();
                m_actionsQueue = m_actionsQueue.Enqueue(async () =>
                {
                    try
                    {
                        await base.FireAsync(trigger);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex) {
                        tcs.TrySetException(ex);
                    }
                });
                m_queueCount++;
                m_queueMonitor.Exit();
                var _ = StartQueueIfNecessaryAsync();
                await tcs.Task;
            }
        }

        public void SkipPending()
        {
            m_queueMonitor.Enter();
            m_actionsQueue = ImmutableQueue<Func<Task>>.Empty;
            m_queueCount = 0;
            m_queueMonitor.Exit();
        }

        private Task StartQueueIfNecessaryAsync(bool lockTaken = false)
        {
            if (lockTaken || m_monitor.TryEnter()) { return ProcessQueueInternal(); }

            return TaskHelpers.CompletedTask;
        }

        private async Task ProcessQueueInternal()
        {
            // Always yield if the task was queued.
            await Task.Yield();

            m_queueMonitor.Enter();
            try
            {
                while (m_queueCount > 0)
                {
                    var current = m_actionsQueue.Peek();
                    m_actionsQueue = m_actionsQueue.Dequeue();
                    m_queueCount--;
                    m_queueMonitor.Exit();

                    try
                    {
                        if (current != null) await current();
                    }
                    finally { m_queueMonitor.Enter(); }
                }
            }
            finally
            {
                // Exit monitor regardless of this method entering the monitor.
                m_monitor.Exit();
                m_queueMonitor.Exit();
            }
        }
    }

    public sealed class QueuedAwaitableStateMachine<TState, TTrigger> :
        QueuedAwaitableStateMachineBase<TState, TTrigger>
    {
        public QueuedAwaitableStateMachine(TState initialState,
            AwaitableConfiguration<TState, TTrigger> awaitableConfiguration)
            : base(initialState, awaitableConfiguration) {}
    }
}