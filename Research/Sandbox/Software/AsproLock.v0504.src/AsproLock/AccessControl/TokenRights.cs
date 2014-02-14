using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum TokenRights
    {
        AssignPrimary     = 1,
        Duplicate         = 2,
        Impersonate       = 4,
        Query             = 8,
        QuerySource       = 16,
        AdjustPrivileges  = 32,
        AdjustGroups      = 64,
        AdjustDefault     = 128,
        AdjustSessionID   = 256,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | Query,
        Write = StandardRights.Write | AdjustPrivileges | AdjustGroups | AdjustDefault,
        Execute = StandardRights.Execute,

        AllAccess = StandardRights.Required | AssignPrimary | Duplicate | Impersonate | Query |
            QuerySource | AdjustPrivileges | AdjustGroups | AdjustDefault | AdjustSessionID
    }
}