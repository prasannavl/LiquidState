// Author: Prasanna V. Loganathar
// Created: 02:20 12-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Threading.Tasks;
using LiquidState.Awaitable.Core;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Awaitable
{
    public abstract class GuardedAwaitableStateMachineBase<TState, TTrigger> :
        RawAwaitableStateMachineBase<TState, TTrigger>
    {
        private InterlockedMonitor monitor = new InterlockedMonitor();

        protected GuardedAwaitableStateMachineBase(TState initialState,
            AwaitableConfiguration<TState, TTrigger> awaitableConfiguration)
            : base(initialState, awaitableConfiguration)
        {
        }

        public override async Task MoveToStateAsync(TState state,
            StateTransitionOption option = StateTransitionOption.Default)
        {
            if (monitor.TryEnter())
            {
                try
                {
                    await base.MoveToStateAsync(state, option).ConfigureAwait(false);
                }
                finally
                {
                    monitor.Exit();
                }
            }
            else
            {
                if (IsEnabled)
                    AwaitableExecutionHelper.ThrowInTransition();
            }
        }

        public override async Task FireAsync<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (monitor.TryEnter())
            {
                try
                {
                    await base.FireAsync(parameterizedTrigger, argument).ConfigureAwait(false);
                }
                finally
                {
                    monitor.Exit();
                }
            }
            else
            {
                if (IsEnabled)
                    AwaitableExecutionHelper.ThrowInTransition();
            }
        }

        public override async Task FireAsync(TTrigger trigger)
        {
            if (monitor.TryEnter())
            {
                try
                {
                    await base.FireAsync(trigger).ConfigureAwait(false);
                }
                finally
                {
                    monitor.Exit();
                }
            }
            else
            {
                if (IsEnabled)
                    AwaitableExecutionHelper.ThrowInTransition();
            }
        }
    }

    public sealed class GuardedAwaitableStateMachine<TState, TTrigger> :
        GuardedAwaitableStateMachineBase<TState, TTrigger>
    {
        public GuardedAwaitableStateMachine(TState initialState,
            AwaitableConfiguration<TState, TTrigger> awaitableConfiguration)
            : base(initialState, awaitableConfiguration)
        {
        }
    }
}