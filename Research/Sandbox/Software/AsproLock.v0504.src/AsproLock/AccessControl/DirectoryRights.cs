using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum DirectoryRights
    {
        ListDirectory       = 1,
        AddFile             = 2,
        AddSubdirectory     = 4,
        ReadEA              = 8,
        WriteEA             = 16,
        Traverse            = 32,
        DeleteChild         = 64,
        ReadAttributes      = 128,
        WriteAttributes     = 256,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | ListDirectory | AddFile,
        Write = StandardRights.Write | AddSubdirectory | ReadEA,
        Execute = StandardRights.Execute | ListDirectory | AddFile,

        AllAccess = ListDirectory | AddFile | AddSubdirectory | ReadEA | StandardRights.Required 
    }
}