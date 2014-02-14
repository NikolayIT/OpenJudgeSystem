using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum FileRights
    {
        ReadData            = 1,
        WriteData           = 2,
        AppendData          = 4,
        ReadEA              = 8,
        WriteEA             = 16,
        ExecuteFile         = 32,
        ReadAttributes      = 128,
        WriteAttributes     = 256,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | ReadData | ReadAttributes | ReadEA  | Synchronize,
        Write  = StandardRights.Write | WriteData | WriteAttributes | WriteEA | AppendData | Synchronize,
        Execute = StandardRights.Execute | ReadAttributes | ExecuteFile | Synchronize,

        AllAccess = StandardRights.Required | Synchronize | ReadData | ReadAttributes | ReadEA  |
            WriteData | WriteAttributes | WriteEA | AppendData | ExecuteFile | 64
    }
}