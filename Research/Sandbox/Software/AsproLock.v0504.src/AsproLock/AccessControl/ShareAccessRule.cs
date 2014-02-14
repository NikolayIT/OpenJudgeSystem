using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    public class ShareAccessRule : AccessRule
    {
        public ShareAccessRule(IdentityReference identity, ShareRights shareRights, AccessControlType type)
            : base(identity, (int)shareRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public ShareAccessRule(string identity, ShareRights shareRights, AccessControlType type)
            : this(new NTAccount(identity), shareRights, type)
        {
        }

        public ShareRights ShareRights 
        {
            get
            {
                return (ShareRights)base.AccessMask;
            }
        }
    }
}
