using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of WaitObjectAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class WaitObjectAccessRule : AccessRule
    {
        public WaitObjectAccessRule(IdentityReference identity, WaitObjectRights waitObjectRights, AccessControlType type)
            : base(identity, (int)waitObjectRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public WaitObjectAccessRule(string identity, WaitObjectRights waitObjectRights, AccessControlType type)
            : this(new NTAccount(identity), waitObjectRights, type)
        {
        }

        public WaitObjectRights WaitObjectRights 
        {
            get
            {
                return (WaitObjectRights)base.AccessMask;
            }
        }
    }
}
