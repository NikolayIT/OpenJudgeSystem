using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum PrinterRights
    {
        ServerAdminister = 1,
        ServerEnumerate = 2,
        PrinterAdminister = 4,
        PrinterUse = 8,
        JobAdminister = 16,
        JobRead = 32,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | PrinterUse,
        Write = StandardRights.Write | PrinterUse,
        Execute = StandardRights.Execute | PrinterUse,

        AllAccess = PrinterAdminister | PrinterUse | StandardRights.Required
    }
}