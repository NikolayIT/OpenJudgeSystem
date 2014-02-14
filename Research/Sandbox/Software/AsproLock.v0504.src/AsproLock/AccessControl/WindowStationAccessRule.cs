using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of WindowStationAccessRights rights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    /// <remarks><para></para></remarks>
    public sealed class WindowStationAccessRule : AccessRule
    {
        public WindowStationAccessRule(IdentityReference identity, WindowStationRights windowStationRights, AccessControlType type)
            : base(identity, (int)windowStationRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public WindowStationAccessRule(string identity, WindowStationRights windowStationRights, AccessControlType type)
            : this(new NTAccount(identity), windowStationRights, type)
        {
        }

        public WindowStationRights WindowStationRights 
        {
            get
            {
                return (WindowStationRights)base.AccessMask;
            }
        }
    }
}
