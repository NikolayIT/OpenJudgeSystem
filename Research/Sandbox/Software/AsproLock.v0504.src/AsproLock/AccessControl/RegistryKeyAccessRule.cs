using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of RegistryKeyAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class RegistryKeyAccessRule : AccessRule
    {
        public RegistryKeyAccessRule(IdentityReference identity, RegistryKeyRights registryKeyRights, AccessControlType type)
            : base(identity, (int)registryKeyRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public RegistryKeyAccessRule(string identity, RegistryKeyRights registryKeyRights, AccessControlType type)
            : this(new NTAccount(identity), registryKeyRights, type)
        {
        }

        public RegistryKeyRights RegistryKeyRights 
        {
            get
            {
                return (RegistryKeyRights)base.AccessMask;
            }
        }
    }
}
