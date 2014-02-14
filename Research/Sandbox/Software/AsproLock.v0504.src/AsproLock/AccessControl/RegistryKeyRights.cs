using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum RegistryKeyRights
    {
        QueryValue          = 1,
        SetValue            = 2,
        CreateSubKey        = 3,
        EnumerateSubKeys    = 4,
        Notify              = 16,
        CreateLink          = 32,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | QueryValue | EnumerateSubKeys | Notify,
        Write = StandardRights.Write | SetValue | CreateSubKey,
        Execute = StandardRights.Execute | QueryValue | EnumerateSubKeys | Notify,
 
        AllAccess = StandardRights.All | QueryValue | SetValue | CreateSubKey | EnumerateSubKeys |
            Notify | CreateLink
   }
}