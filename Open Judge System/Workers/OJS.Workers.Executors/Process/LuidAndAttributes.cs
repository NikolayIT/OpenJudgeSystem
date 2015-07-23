namespace OJS.Workers.Executors.Process
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The LUID_AND_ATTRIBUTES structure represents a locally unique identifier (LUID) and its attributes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LuidAndAttributes
    {
        public Luid Luid;

        public uint Attributes;
    }
}
