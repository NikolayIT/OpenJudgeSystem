using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum FileMappingRights
    {
        FileMapCopy = 1,
        FileMapWrite = 2,
        FileMapRead = 4,
        SectionMapExecute = 8,
        SectionExtendSize = 16,
        FileMapExecute = 32,
        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | FileMapCopy | FileMapRead,
        Write = StandardRights.Write | FileMapWrite,
        Execute = StandardRights.Execute | FileMapExecute,

        AllAccess = FileMapCopy | FileMapWrite | FileMapRead | SectionMapExecute | SectionExtendSize |
            StandardRights.Required 
    }
}