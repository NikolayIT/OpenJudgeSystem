namespace OJS.Workers.Executors.JobObjects
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains basic user-interface restrictions for a job object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BasicUiRestrictions
    {
        /// <summary>
        /// The restriction class for the user interface.
        /// </summary>
        public uint UIRestrictionsClass { get; set; }
    }
}
