using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum JobObjectRights
    {
        AssignProcess            = 1,
        SetAttributes            = 2,
        Query                    = 4,
        Terminate                = 8,
        SetSecurityAttributes    = 16,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | Query,
        Write = StandardRights.Write | AssignProcess | SetAttributes | Terminate,
        Execute = StandardRights.Execute | Synchronize,

        AllAccess = AssignProcess | SetAttributes | Query | Terminate | SetSecurityAttributes |
            StandardRights.Required | Synchronize
    }
}