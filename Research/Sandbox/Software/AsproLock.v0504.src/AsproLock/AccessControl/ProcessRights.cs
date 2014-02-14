using System;

namespace Asprosys.Security.AccessControl
{
    [Flags]
    public enum ProcessRights
    {
        Terminate        = 1,  
        CreateThread     = 2,  
        SetSessionID     = 4,  
        VMOperation      = 8,  
        VMRead           = 16,  
        VMWrite          = 32,  
        DuplicateHandle  = 64,  
        CreateProcess    = 128,  
        SetQuota         = 256,  
        SetInformation   = 512,  
        QueryInformation = 1024,  
        SuspendResume    = 2048,  

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,
        Synchronize = 0x00100000,

        Read = StandardRights.Read | VMRead | QueryInformation,
        Write = StandardRights.Write | CreateProcess | CreateThread | VMOperation | VMWrite |
            DuplicateHandle | Terminate | SetQuota | SetInformation,
        Execute = StandardRights.Execute | Synchronize,

        AllAccess = StandardRights.Required | Synchronize | Terminate | CreateThread | 
            SetSessionID | VMOperation | VMRead | VMWrite | DuplicateHandle | 
            CreateProcess | SetQuota | SetInformation | QueryInformation | SuspendResume
    }
}