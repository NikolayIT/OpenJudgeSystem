using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum DesktopRights
    {
        /// <summary>
        /// 
        /// </summary>
        ReadObjects       = 1,
        /// <summary>
        /// 
        /// </summary>
        CreateWindow      = 2,
        /// <summary>
        /// 
        /// </summary>
        CreateMenu        = 4,
        /// <summary>
        /// 
        /// </summary>
        HookControl       = 8,
        /// <summary>
        /// 
        /// </summary>
        JournalRecord     = 16,
        /// <summary>
        /// 
        /// </summary>
        JournalPlayback   = 32,
        /// <summary>
        /// 
        /// </summary>
        Enumerate         = 64,
        /// <summary>
        /// 
        /// </summary>
        WriteObjects      = 128,
        /// <summary>
        /// 
        /// </summary>
        SwitchDesktop     = 256,


        /// <summary>
        /// 
        /// </summary>f
        Delete = 0x00010000,
        /// <summary>
        /// 
        /// </summary>
        ReadPermissions = 0x00020000,
        /// <summary>
        /// 
        /// </summary>
        WritePermissions = 0x00040000,
        /// <summary>
        /// 
        /// </summary>
        TakeOwnership = 0x00080000,
        /// <summary>
        /// 
        /// </summary>
        Synchronize = 0x00100000,

        /// <summary>
        /// 
        /// </summary>
        Read = ReadObjects | Enumerate | StandardRights.Read,
        /// <summary>
        /// 
        /// </summary>
        Write = WriteObjects | CreateWindow | CreateMenu | HookControl | JournalRecord |
            JournalPlayback | StandardRights.Write,
        /// <summary>
        /// 
        /// </summary>
        Execute = SwitchDesktop | StandardRights.Execute,

        /// <summary>
        /// 
        /// </summary>
        AllAccess = ReadObjects | CreateWindow | CreateMenu | HookControl |
            JournalRecord | JournalPlayback | Enumerate | WriteObjects | SwitchDesktop |
            StandardRights.Required
    }
}