namespace OJS.Workers.Executors.Process
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The SidIdentifierAuthority structure represents the top-level authority of a security identifier (SID).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SidIdentifierAuthority
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.I1)]
        public byte[] Value;

        public SidIdentifierAuthority(byte[] value)
        {
            this.Value = value;
        }
    }
}
