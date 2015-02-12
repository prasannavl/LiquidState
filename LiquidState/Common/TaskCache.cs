// Author: Prasanna V. Loganathar
// Created: 3:49 PM 07-12-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Threading.Tasks;

namespace LiquidState.Common
{
    internal class TaskCache
    {
        public static Task Completed = Task.FromResult(false);
    }
}