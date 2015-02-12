// Author: Prasanna V. Loganathar
// Created: 1:30 AM 05-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LiquidState.Common;
using LiquidState.Configuration;

namespace LiquidState.Machines
{
    public class AsyncStateMachine<TState, TTrigger> : IAwaitableStateMachine<TState, TTrigger>
    {
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

        private readonly AwaitableStateMachine<TState, TTrigger> machine;
        private IImmutableQueue<Func<Task>> actionsQueue;
        private int queueCount;
        private InterlockedMonitor queueMonitor = new InterlockedMonitor();

        internal AsyncStateMachine(TState initialState, AwaitableStateMachineConfiguration<TState, TTrigger> config)
        {
            Contract.Requires(initialState != null);
            Contract.Requires(config != null);

            machine = new AwaitableStateMachine<TState, TTrigger>(initialState, config);
            actionsQueue = ImmutableQueue.Create<Func<Task>>();
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

        public bool CanHandleTrigger(TTrigger trigger)
        {
            return machine.CanHandleTrigger(trigger);
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
                            var _ = ProcessQueueAsync(true, true);
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
                        await machine.MoveToState(state, option);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
                queueCount++;
                queueMonitor.Exit();
                var _ = ProcessQueueAsync();
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
            var _ = ProcessQueueAsync();
        }

        public async Task Stop()
        {
            if (Interlocked.CompareExchange(ref machine.isEnabled, 0, 1) == 1)
            {
                machine.Monitor.EnterWithHybridSpin();
                SkipPending();
                try
                {
                    await machine.PerformStopTransitionAsync();
                }
                finally
                {
                    machine.Monitor.Exit();
                }
            }
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
                            var _ = ProcessQueueAsync(true, true);
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
                        tcs.SetException(ex);
                    }
                });
                queueCount++;
                queueMonitor.Exit();
                var _ = ProcessQueueAsync();
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
                            var _ = ProcessQueueAsync(true, true);
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
                        tcs.SetException(ex);
                    }
                });
                queueCount++;
                queueMonitor.Exit();
                var _ = ProcessQueueAsync();
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

        private async Task ProcessQueueAsync(bool shouldYield = true, bool lockTaken = false)
        {
            if (lockTaken || machine.Monitor.TryEnter())
            {
                if (shouldYield) await Task.Yield();
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
                    machine.Monitor.Exit();
                    queueMonitor.Exit();
                }
            }
        }
    }
}