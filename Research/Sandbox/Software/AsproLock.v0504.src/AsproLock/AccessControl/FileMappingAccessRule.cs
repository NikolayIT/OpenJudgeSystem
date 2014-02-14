using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of FileMappingAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class FileMappingAccessRule : AccessRule
    {
        public FileMappingAccessRule(IdentityReference identity, FileMappingRights fileMappingRights, AccessControlType type)
            : base(identity, (int)fileMappingRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public FileMappingAccessRule(string identity, FileMappingRights fileMappingRights, AccessControlType type)
            : this(new NTAccount(identity), fileMappingRights, type)
        {
        }

        public FileMappingRights FileMappingRights 
        {
            get
            {
                return (FileMappingRights)base.AccessMask;
            }
        }
    }
}
