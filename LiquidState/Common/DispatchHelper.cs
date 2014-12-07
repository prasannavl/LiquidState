using System.Threading;

namespace LiquidState.Common
{
    public static class DispatchHelper
    {
        public static IDispatcher Current { get; set; }

        static DispatchHelper()
        {
            Current = new SynchronizationContextDispatcher();
        }
    }
}
