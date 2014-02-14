using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of FileMappingAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class ThreadAccessRule : AccessRule
    {
        public ThreadAccessRule(IdentityReference identity, ThreadRights threadRights, AccessControlType type)
            : base(identity, (int)threadRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public ThreadAccessRule(string identity, ThreadRights threadRights, AccessControlType type)
            : this(new NTAccount(identity), threadRights, type)
        {
        }

        public ThreadRights ThreadRights 
        {
            get
            {
                return (ThreadRights)base.AccessMask;
            }
        }
    }
}
