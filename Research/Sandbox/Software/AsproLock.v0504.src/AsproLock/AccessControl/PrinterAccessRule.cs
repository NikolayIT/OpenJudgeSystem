using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    public class PrinterAccessRule : AccessRule
    {
        public PrinterAccessRule(IdentityReference identity, PrinterRights serviceRights, AccessControlType type)
            : base(identity, (int)serviceRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public PrinterAccessRule(string identity, PrinterRights serviceRights, AccessControlType type)
            : this(new NTAccount(identity), serviceRights, type)
        {
        }

        public PrinterRights ServiceRights 
        {
            get
            {
                return (PrinterRights)base.AccessMask;
            }
        }
    }
}
