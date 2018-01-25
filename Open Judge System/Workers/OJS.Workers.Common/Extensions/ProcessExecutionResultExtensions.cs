namespace OJS.Workers.Common.Extensions
{
    using System;

    public static class ProcessExecutionResultExtensions
    {
        public static void OffsetTimeWorked(this ProcessExecutionResult result, int baseTimeUsed)
        {
            // Display the TimeWorked, when the process was killed for being too slow (TotalProcessorTime is still usually under the timeLimit when a process is killed),
            // otherwise display TotalProcessorTime, so that the final result is as close as possible to the actual worker time
            if (result.ProcessWasKilled)
            {
                result.TimeWorked = result.TimeWorked.TotalMilliseconds > baseTimeUsed
                    ? result.TimeWorked - TimeSpan.FromMilliseconds(baseTimeUsed)
                    : result.TimeWorked;
            }
            else
            {
                result.TimeWorked = result.TotalProcessorTime.TotalMilliseconds > baseTimeUsed
                    ? result.TotalProcessorTime - TimeSpan.FromMilliseconds(baseTimeUsed)
                    : result.TotalProcessorTime;
            }
        }

        public static void OffsetMemoryUsed(this ProcessExecutionResult result, int baseMemoryUsed) =>
            result.MemoryUsed = Math.Max(result.MemoryUsed - baseMemoryUsed, baseMemoryUsed);
    }
}