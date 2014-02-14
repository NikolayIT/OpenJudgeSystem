using System;

namespace Asprosys.Security.AccessControl
{
    public enum ServiceRights
    {
        QueryConfiguration = 1,
        ChangeConfiguration = 2,
        QueryStatus = 4,
        EnumerateDependents = 8,
        ServiceStart = 16,
        ServiceStop = 32,
        ServicePauseAndContinue = 64,
        ServiceInterogate = 128,
        SendCustomControl = 256,

        Delete = 0x00010000,
        ReadPermissions = 0x00020000,
        WritePermissions = 0x00040000,
        TakeOwnership = 0x00080000,

        Read = StandardRights.Read | QueryConfiguration | QueryStatus | EnumerateDependents 
            | ServiceInterogate | SendCustomControl,
        Write = StandardRights.Write | ChangeConfiguration,
        Execute = StandardRights.Execute | ServiceStart | ServiceStop | ServicePauseAndContinue 
            | ServiceInterogate | SendCustomControl,

        AllAccess = QueryConfiguration | ChangeConfiguration | QueryStatus | EnumerateDependents
          | ServiceStart | ServiceStop | ServicePauseAndContinue | ServiceInterogate | SendCustomControl
          | StandardRights.Required
    }
}
