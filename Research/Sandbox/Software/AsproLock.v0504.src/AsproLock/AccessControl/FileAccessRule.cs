using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of FileAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class FileAccessRule : AccessRule
    {
        public FileAccessRule(IdentityReference identity, FileRights fileRights, AccessControlType type)
            : base(identity, (int)fileRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public FileAccessRule(string identity, FileRights fileRights, AccessControlType type)
            : this(new NTAccount(identity), fileRights, type)
        {
        }

        public FileRights FileRights 
        {
            get
            {
                return (FileRights)base.AccessMask;
            }
        }
    }
}
