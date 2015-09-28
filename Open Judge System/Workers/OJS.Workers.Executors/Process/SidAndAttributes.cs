namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The structure represents a security identifier (SID) and its attributes. SIDs are used to uniquely identify users or groups.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SidAndAttributes
    {
        public IntPtr Sid;

        public uint Attributes;
    }
}