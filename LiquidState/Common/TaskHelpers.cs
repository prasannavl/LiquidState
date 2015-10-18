// Author: Prasanna V. Loganathar
// Created: 03:14 12-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Threading.Tasks;

namespace LiquidState.Common
{
    internal class TaskHelpers
    {
        public static Task CompletedTask = Task.FromResult(true);
    }
}