using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LiquidState.Common
{
    public interface IDispatcher
    {
        void Initialize();
        TaskScheduler Scheduler { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool CheckAccess();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Execute(Action action);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Execute<T>(Action<T> action, T state);
    }
}
