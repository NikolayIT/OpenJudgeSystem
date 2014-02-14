using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of TokenAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class TokenAccessRule : AccessRule
    {
        public TokenAccessRule(IdentityReference identity, TokenRights tokenRights, AccessControlType type)
            : base(identity, (int)tokenRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public TokenAccessRule(string identity, TokenRights tokenRights, AccessControlType type)
            : this(new NTAccount(identity), tokenRights, type)
        {
        }

        public TokenRights TokenRights 
        {
            get
            {
                return (TokenRights)base.AccessMask;
            }
        }
    }
}
