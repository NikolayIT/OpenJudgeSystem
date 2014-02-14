using System;

namespace Asprosys.Security.AccessControl
{
    public enum ShareRights
    {
        FileRead    =  1, // user has read access
        FileWrite   =  2, // user has write access
        FileCreate  =  4, // user has create access

        Read = StandardRights.Read | FileRead,
        Write = StandardRights.Write | FileWrite | FileCreate,
        Execute = StandardRights.Execute | FileCreate,

        AllAccess = FileRead | FileWrite | FileCreate | StandardRights.Required
    }
}
