namespace OJS.Workers.Executors.Process
{
    using System;

    [Flags]
    public enum PriorityClass
    {
        /// <summary>
        /// Process that has priority above NORMAL_PRIORITY_CLASS but below HIGH_PRIORITY_CLASS.
        /// </summary>
        ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000,

        /// <summary>
        /// Process that has priority above IDLE_PRIORITY_CLASS but below NORMAL_PRIORITY_CLASS.
        /// </summary>
        BELOW_NORMAL_PRIORITY_CLASS = 0x00004000,

        /// <summary>
        /// Process that performs time-critical tasks that must be executed immediately for it to run correctly. The threads of a high-priority class process preempt the threads of normal or idle priority class processes. An example is the Task List, which must respond quickly when called by the user, regardless of the load on the operating system. Use extreme care when using the high-priority class, because a high-priority class CPU-bound application can use nearly all available cycles.
        /// </summary>
        HIGH_PRIORITY_CLASS = 0x00000080,

        /// <summary>
        /// Process whose threads run only when the system is idle and are preempted by the threads of any process running in a higher priority class. An example is a screen saver. The idle priority class is inherited by child processes.
        /// </summary>
        IDLE_PRIORITY_CLASS = 0x00000040,

        /// <summary>
        /// Process with no special scheduling needs.
        /// </summary>
        NORMAL_PRIORITY_CLASS = 0x00000020,

        /// <summary>
        /// Process that has the highest possible priority. The threads of a real-time priority class process preempt the threads of all other processes, including operating system processes performing important tasks. For example, a real-time process that executes for more than a very brief interval can cause disk caches not to flush or cause the mouse to be unresponsive.
        /// </summary>
        REALTIME_PRIORITY_CLASS = 0x00000100,
    }
}
