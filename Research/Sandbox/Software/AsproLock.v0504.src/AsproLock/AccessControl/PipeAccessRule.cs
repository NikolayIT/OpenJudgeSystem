using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of PipeAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class PipeAccessRule : AccessRule
    {
        public PipeAccessRule(IdentityReference identity, PipeRights pipeRights, AccessControlType type)
            : base(identity, (int)pipeRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public PipeAccessRule(string identity, PipeRights pipeRights, AccessControlType type)
            : this(new NTAccount(identity), pipeRights, type)
        {
        }

        public PipeRights PipeRights 
        {
            get
            {
                return (PipeRights)base.AccessMask;
            }
        }
    }
}
