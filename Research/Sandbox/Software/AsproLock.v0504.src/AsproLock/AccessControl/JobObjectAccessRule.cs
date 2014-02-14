using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of JobObjectAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class JobObjectAccessRule : AccessRule
    {
        public JobObjectAccessRule(IdentityReference identity, JobObjectRights jobObjectRights, AccessControlType type)
            : base(identity, (int)jobObjectRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public JobObjectAccessRule(string identity, JobObjectRights jobObjectRights, AccessControlType type)
            : this(new NTAccount(identity), jobObjectRights, type)
        {
        }

        public JobObjectRights JobObjectRights 
        {
            get
            {
                return (JobObjectRights)base.AccessMask;
            }
        }
    }
}
