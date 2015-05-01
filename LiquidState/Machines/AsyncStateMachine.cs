// Author: Prasanna V. Loganathar
// Created: 1:30 AM 05-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LiquidState.Common;
using LiquidState.Configuration;

namespace LiquidState.Machines
{
    public class AsyncStateMachine<TState, TTrigger> : IAwaitableStateMachine<TState, TTrigger>
    {
        private readonly AwaitableStateMachine<TState, TTrigger> machine;
        private IImmutableQueue<Func<Task>> actionsQueue;
        private int queueCount;
        private InterlockedBlockingMonitor queueMonitor = new InterlockedBlockingMonitor();

        internal AsyncStateMachine(TState initialState, AwaitableStateMachineConfiguration<TState, TTrigger> config)
        {
            Contract.Requires(initialState != null);
            Contract.Requires(config != null);

            machine = new AwaitableStateMachine<TState, TTrigger>(initialState, config);
            actionsQueue = ImmutableQueue.Create<Func<Task>>();
        }

        public event Action<TTrigger, TState> UnhandledTriggerExecuted
        {
            add { machine.UnhandledTriggerExecuted += value; }
            remove { machine.UnhandledTriggerExecuted -= value; }
        }

        public event Action<TState, TState> StateChanged
        {
            add { machine.StateChanged += value; }
            remove { machine.StateChanged -= value; }
        }

        public Task<bool> CanHandleTriggerAsync(TTrigger trigger)
        {
            return machine.CanHandleTriggerAsync(trigger);
        }

        public bool CanTransitionTo(TState state)
        {
            return machine.CanTransitionTo(state);
        }

        public async Task MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            if (!IsEnabled)
                return;

            var flag = true;

            queueMonitor.Enter();
            if (machine.Monitor.TryEnter())
            {
                if (queueCount == 0)
                {
                    queueMonitor.Exit();
                    flag = false;

                    try
                    {
                        await machine.MoveToStateInternal(state, option);
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
                            machine.Monitor.Exit();
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
                        await machine.MoveToStateInternal(state, option);
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

        public void Pause()
        {
            machine.Pause();
        }

        public void Resume()
        {
            machine.Resume();
            var _ = StartQueueIfNecessaryAsync();
        }

        public async Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (!IsEnabled)
                return;

            var flag = true;

            queueMonitor.Enter();
            if (machine.Monitor.TryEnter())
            {
                if (queueCount == 0)
                {
                    queueMonitor.Exit();
                    flag = false;

                    try
                    {
                        await machine.FireInternalAsync(parameterizedTrigger, argument);
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
                            machine.Monitor.Exit();
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
                        await machine.FireInternalAsync(parameterizedTrigger, argument);
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

        public async Task FireAsync(TTrigger trigger)
        {
            if (!IsEnabled)
                return;

            var flag = true;

            queueMonitor.Enter();
            if (machine.Monitor.TryEnter())
            {
                if (queueCount == 0)
                {
                    queueMonitor.Exit();
                    flag = false;

                    try
                    {
                        await machine.FireInternalAsync(trigger);
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
                            machine.Monitor.Exit();
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
                        await machine.FireInternalAsync(trigger);
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

        public bool IsInTransition
        {
            get { return machine.IsInTransition; }
        }

        public TState CurrentState
        {
            get { return machine.CurrentState; }
        }

        public IEnumerable<TTrigger> CurrentPermittedTriggers
        {
            get { return machine.CurrentPermittedTriggers; }
        }

        public bool IsEnabled
        {
            get { return machine.IsEnabled; }
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
            if (lockTaken || machine.Monitor.TryEnter())
            {
                return ProcessQueueInternal();
            }

            return Task.FromResult(true);
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
                machine.Monitor.Exit();
                queueMonitor.Exit();
            }
        }
    }
}
