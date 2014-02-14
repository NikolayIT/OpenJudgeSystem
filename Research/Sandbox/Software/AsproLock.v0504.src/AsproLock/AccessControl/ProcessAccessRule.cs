using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of FileMappingAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class ProcessAccessRule : AccessRule
    {
        public ProcessAccessRule(IdentityReference identity, ProcessRights processRights, AccessControlType type)
            : base(identity, (int)processRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public ProcessAccessRule(string identity, ProcessRights processRights, AccessControlType type)
            : this(new NTAccount(identity), processRights, type)
        {
        }

        public ProcessRights ProcessRights 
        {
            get
            {
                return (ProcessRights)base.AccessMask;
            }
        }
    }
}
