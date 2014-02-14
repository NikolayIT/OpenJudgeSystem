using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    public class ServiceAccessRule : AccessRule
    {
        public ServiceAccessRule(IdentityReference identity, ServiceRights serviceRights, AccessControlType type)
            : base(identity, (int)serviceRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public ServiceAccessRule(string identity, ServiceRights serviceRights, AccessControlType type)
            : this(new NTAccount(identity), serviceRights, type)
        {
        }

        public ServiceRights ServiceRights 
        {
            get
            {
                return (ServiceRights)base.AccessMask;
            }
        }
    }
}
