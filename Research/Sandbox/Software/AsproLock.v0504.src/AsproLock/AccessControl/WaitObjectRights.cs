using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum WaitObjectRights
    {
        QueryState = 1,
        ModifyState = 2,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | QueryState,
        Write = StandardRights.Write | ModifyState,
        Execute = StandardRights.Execute | Synchronize,

        AllAccess = ModifyState | QueryState | StandardRights.Required | Synchronize 
    }
}