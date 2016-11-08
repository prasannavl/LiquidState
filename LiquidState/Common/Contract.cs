using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LiquidState.Common
{
    public static class Contract
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull<T>(T obj, string paramName)
        {
            if (obj == null) throw new ArgumentNullException(paramName);
        }
    }
}