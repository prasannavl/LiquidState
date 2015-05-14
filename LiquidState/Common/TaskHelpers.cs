using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiquidState.Common
{
    internal class TaskHelpers
    {
        public static Task CompletedTask = Task.FromResult(true);
    }
}
