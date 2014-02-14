using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum ThreadRights
    {
        Terminate              = 0001,  
        SuspendResume          = 0002,  
        GetContext             = 0008,  
        SetContext             = 0010,  
        SetInformation         = 0020,  
        QueryInformation       = 0040,  
        SetThreadToken         = 0080,
        Impersonate            = 0100,
        DirectImpersonation    = 0200,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | GetContext | QueryInformation,
        Write = StandardRights.Write | Terminate | SuspendResume | SetInformation | SetContext,
        Execute = StandardRights.Execute | Synchronize,

        AllAccess =  StandardRights.Required | Synchronize | Terminate | SuspendResume | 
            GetContext | SetContext | SetInformation | QueryInformation | SetThreadToken | 
            Impersonate | DirectImpersonation 
    }
}