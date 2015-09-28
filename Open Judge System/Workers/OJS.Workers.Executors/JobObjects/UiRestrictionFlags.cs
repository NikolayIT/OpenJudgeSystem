namespace OJS.Workers.Executors.JobObjects
{
    using System;

    /// <summary>
    /// The restriction class for the user interface.
    /// <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms684152%28v=vs.85%29.aspx">More information</see>
    /// </summary>
    [Flags]
    public enum UiRestrictionFlags
    {
        /// <summary>
        /// Prevents processes associated with the job from using USER handles owned by processes not associated with the same job.
        /// </summary>
        /// <remarks>
        /// If you specify the JOB_OBJECT_UILIMIT_HANDLES flag, when a process associated with the job broadcasts messages, they are only sent to top-level windows owned by processes associated with the same job. In addition, hooks can be installed only on threads belonging to processes associated with the job.
        /// </remarks>
        JOB_OBJECT_UILIMIT_HANDLES = 0x00000001,

        /// <summary>
        /// Prevents processes associated with the job from reading data from the clipboard.
        /// </summary>
        JOB_OBJECT_UILIMIT_READCLIPBOARD = 0x00000002,

        /// <summary>
        /// Prevents processes associated with the job from writing data to the clipboard.
        /// </summary>
        JOB_OBJECT_UILIMIT_WRITECLIPBOARD = 0x00000004,

        /// <summary>
        /// Prevents processes associated with the job from changing system parameters by using the SystemParametersInfo function.
        /// </summary>
        JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS = 0x00000008,

        /// <summary>
        /// Prevents processes associated with the job from calling the ChangeDisplaySettings function.
        /// </summary>
        JOB_OBJECT_UILIMIT_DISPLAYSETTINGS = 0x00000010,

        /// <summary>
        /// Prevents processes associated with the job from accessing global atoms. When this flag is used, each job has its own atom table.
        /// </summary>
        JOB_OBJECT_UILIMIT_GLOBALATOMS = 0x00000020,

        /// <summary>
        /// Prevents processes associated with the job from creating desktops and switching desktops using the CreateDesktop and SwitchDesktop functions.
        /// </summary>
        JOB_OBJECT_UILIMIT_DESKTOP = 0x00000040,

        /// <summary>
        /// Prevents processes associated with the job from calling the ExitWindows or ExitWindowsEx function.
        /// </summary>
        JOB_OBJECT_UILIMIT_EXITWINDOWS = 0x00000080,
    }
}
