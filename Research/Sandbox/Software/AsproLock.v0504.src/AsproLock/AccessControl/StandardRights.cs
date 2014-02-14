using System;
using System.Collections.Generic;
using System.Text;

namespace Asprosys.Security.AccessControl
{
    public enum StandardRights
    {
        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = ReadPermissions,
        Write = ReadPermissions,
        Execute = ReadPermissions,
        Required = Delete | ReadPermissions | WritePermissions | TakeOwnership,

        All = Delete | ReadPermissions | WritePermissions | TakeOwnership | Synchronize 
    }
}
