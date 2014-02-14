using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum PipeRights
    {
        ReadData            = 1,
        WriteData           = 2,
        CreatePipeInstance  = 4,
        ReadAttributes      = 128,
        WriteAttributes     = 256,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | ReadData | CreatePipeInstance | ReadAttributes,
        Write = StandardRights.Write | WriteData |CreatePipeInstance,
        Execute = StandardRights.Execute,

        AllAccess = ReadData | WriteData | CreatePipeInstance | ReadAttributes | WriteAttributes |
            StandardRights.Required | Synchronize
    }
}