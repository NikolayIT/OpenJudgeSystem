using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum WindowStationRights
    {
        EnumDesktops         = 1,
        ReadAttributes       = 2,
        AccessClipboard      = 4,
        CreateDesktop        = 8,
        WriteAttributes      = 16,
        AccessGlobalAtoms    = 32,
        ExitWindows          = 64,
        Enumerate            = 256,
        ReadScreen           = 512,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | EnumDesktops | ReadAttributes | Enumerate | ReadScreen,
        Write = StandardRights.Write | AccessClipboard | CreateDesktop | WriteAttributes,
        Execute = StandardRights.Execute | AccessGlobalAtoms | ExitWindows,

        AllAccess = EnumDesktops  | ReadAttributes  | AccessClipboard | CreateDesktop | 
            WriteAttributes | AccessGlobalAtoms | ExitWindows | Enumerate | ReadScreen |
            StandardRights.Required
    }
}