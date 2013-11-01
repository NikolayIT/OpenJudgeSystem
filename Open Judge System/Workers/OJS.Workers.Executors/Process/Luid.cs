namespace OJS.Workers.Executors.Process
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// An LUID is a 64-bit value guaranteed to be unique only on the system on which it was generated. The uniqueness of a locally unique identifier (LUID) is guaranteed only until the system is restarted.
    /// Applications must use functions and structures to manipulate LUID values.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Luid
    {
        public uint LowPart;

        public long HighPart;
    }
}
