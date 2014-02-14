using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of DirectoryAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class DirectoryAccessRule : AccessRule
    {
        public DirectoryAccessRule(IdentityReference identity, DirectoryRights directoryRights, AccessControlType type)
            : base(identity, (int)directoryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public DirectoryAccessRule(string identity, DirectoryRights directoryRights, AccessControlType type)
            : this(new NTAccount(identity), directoryRights, type)
        {
        }

        public DirectoryRights DirectoryRights 
        {
            get
            {
                return (DirectoryRights)base.AccessMask;
            }
        }
    }
}
