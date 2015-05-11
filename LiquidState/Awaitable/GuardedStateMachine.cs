// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Awaitable.Core;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Awaitable
{
    public abstract class GuardedStateMachineBase<TState, TTrigger> : RawStateMachineBase<TState, TTrigger>
    {
        private InterlockedMonitor monitor = new InterlockedMonitor();

        protected GuardedStateMachineBase(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);
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
                    ExecutionHelper.ThrowInTransition();
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
                    ExecutionHelper.ThrowInTransition();
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
                    ExecutionHelper.ThrowInTransition();
            }
        }
    }

    public sealed class GuardedStateMachine<TState, TTrigger> : GuardedStateMachineBase<TState, TTrigger>
    {
        public GuardedStateMachine(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);
        }
    }
}
