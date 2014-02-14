using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// The base class for all security objects.
    /// </summary>
    public abstract class BaseSecurity : NativeObjectSecurity, IDisposable
    {
        private string m_Name;
        private GenericSafeHandle m_Handle;
        private StandardRights m_HandleRights = StandardRights.ReadPermissions;

        private const int MaximumAccessAllowed = 0x02000000;

        #region Constructors


        internal BaseSecurity(bool isContainer)
            : base(isContainer, ResourceType.Unknown)
        {
        }

        internal BaseSecurity(ResourceType resType, bool isContainer)
            : base(isContainer, resType)
        {
        }

        internal BaseSecurity(string objectName, ResourceType resType, AccessControlSections sectionsRequested, bool isContainer)
            : base(isContainer, resType, objectName, sectionsRequested)
        {
            m_Name = objectName;
        }

        internal BaseSecurity(GenericSafeHandle objectHandle, ResourceType resType, AccessControlSections sectionsRequested, bool isContainer)
            : base(isContainer, resType, objectHandle, sectionsRequested)
        {
            m_Handle = objectHandle;
        }

        #endregion

        #region Persistence methods

        /// <summary>
        /// Persists the changes made to the security object. Must be overridden for 
        /// custom security objects.
        /// </summary>
        public virtual void AcceptChanges()
        {
            if (m_Name == null && m_Handle == null)
                throw new InvalidOperationException("Not associated with a valid Securable Object.");

            base.WriteLock();
            try
            {
                AccessControlSections sectionsChanged = GetSectionsChanged();
                if (sectionsChanged != AccessControlSections.None)
                {
                    if (m_Name != null)
                    {
                        base.Persist(m_Name, sectionsChanged);
                    }
                    else
                    {
                        MakeWriteableHandle(sectionsChanged);
                        base.Persist(m_Handle, sectionsChanged);
                    }
                    ClearSectionsChanged();
                }
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        /// <summary>
        /// Gets the access control sections that have been changed.
        /// </summary>
        /// <returns></returns>
        protected AccessControlSections GetSectionsChanged()
        {
            AccessControlSections sectionsChanged = AccessControlSections.None;

            if (base.OwnerModified) sectionsChanged |= AccessControlSections.Owner;
            if (base.GroupModified) sectionsChanged |= AccessControlSections.Group;
            if (base.AccessRulesModified) sectionsChanged |= AccessControlSections.Access;
            if (base.AuditRulesModified) sectionsChanged |= AccessControlSections.Audit;

            return sectionsChanged;
        }

        /// <summary>
        /// Resets the sections changed flags.
        /// </summary>
        protected void ClearSectionsChanged()
        {
            base.OwnerModified = false;
            base.GroupModified = false;
            base.AccessRulesModified = false;
            base.AuditRulesModified = false;
        }


        #endregion

        #region Handle Methods

        private void MakeWriteableHandle(AccessControlSections sectionsToWrite)
        {

            StandardRights rightsRequired = m_HandleRights;
            bool newHandleRequired = false;

            if ((sectionsToWrite & AccessControlSections.Access) != 0 || (sectionsToWrite & AccessControlSections.Group) != 0)
            {
                if ((m_HandleRights & StandardRights.WritePermissions) == 0)
                {
                    rightsRequired |= StandardRights.WritePermissions;
                    newHandleRequired = true;
                }
            }
            if ((sectionsToWrite & AccessControlSections.Owner) != 0)
            {
                if ((m_HandleRights & StandardRights.TakeOwnership) == 0)
                {
                    rightsRequired |= StandardRights.TakeOwnership;
                    newHandleRequired = true;
                }
            }
            if ((sectionsToWrite & AccessControlSections.Audit) != 0)
            {
                if ((m_HandleRights & (StandardRights)NativeConstants.ACCESS_SYSTEM_SECURITY) == 0)
                {
                    rightsRequired |= (StandardRights)NativeConstants.ACCESS_SYSTEM_SECURITY;
                    newHandleRequired = true;
                }
            }

            if (newHandleRequired)
            {
                IntPtr writeHandle = NativeMethods.DuplicateHandle(m_Handle.DangerousGetHandle(), (int)rightsRequired);
                if (writeHandle == IntPtr.Zero)
                {
                    int err = Marshal.GetLastWin32Error();
                    switch (err)
                    {
                        case NativeConstants.ERROR_ACCESS_DENIED:
                            throw new UnauthorizedAccessException();
                        default:
                            throw new System.ComponentModel.Win32Exception(err);
                    }
                }

                try
                {
                    m_Handle = new GenericSafeHandle(writeHandle, m_Handle);
                    m_HandleRights = rightsRequired;
                }
                catch
                {
                    //Should only happen if out of memory. Release new handle and rethrow.
                    NativeMethods.CloseHandle(writeHandle);
                    throw;
                }
            }
        }

        internal static GenericSafeHandle GetReadHandle(IntPtr handle)
        {
            return GetReadHandle(handle, NativeMethods.CloseHandle);
        }

        internal static GenericSafeHandle GetReadHandle(IntPtr handle, ReleaseHandleCallback releaseCallback)
        {
            IntPtr readHandle = NativeMethods.DuplicateHandle(handle, (int)StandardRights.ReadPermissions);
            if (readHandle == IntPtr.Zero)
            {
                int err = Marshal.GetLastWin32Error();
                switch (err)
                {
                    case NativeConstants.ERROR_ACCESS_DENIED:
                        throw new UnauthorizedAccessException();
                    default:
                        throw new System.ComponentModel.Win32Exception(err);
                }
            }
            return new GenericSafeHandle(readHandle, releaseCallback);
        }

        #endregion

        #region Access Methods

        /// <summary>
        /// Gets the generic mapping associated with the securable object.
        /// </summary>
        /// <returns>A <see cref="GenericMapping"/> associated with this security
        /// object type.</returns>
        protected abstract GenericMapping GetGenericMapping();

        /// <summary> 
        /// Gets a bitmask representing the maximum access allowed to this 
        /// securable object.
        /// </summary>
        /// <param name="tokenHandle">The handle to the token for which the 
        /// access is being calculated. This must be an impersonation token.</param>
        /// <returns>An <see cref="Int32"/> containing the bitmask of the access 
        /// rights that have been granted to the user represented by the token.</returns>
        public int GetMaximumAccessAllowed(SafeHandle tokenHandle)
        {
            int privilegeLength = 4096;
            byte[] accessCheckBuffer = new byte[privilegeLength];
            int accessGranted;
            bool accessAllowed;

            if (!NativeMethods.AccessCheck(base.GetSecurityDescriptorBinaryForm(), tokenHandle,
                MaximumAccessAllowed, GetGenericMapping(), accessCheckBuffer, out privilegeLength, 
                out accessGranted, out accessAllowed))
            {
                int err = Marshal.GetLastWin32Error();
                switch (err)
                {
                    case NativeConstants.ERROR_ACCESS_DENIED:
                        throw new UnauthorizedAccessException();
                    case NativeConstants.ERROR_NO_IMPERSONATION_TOKEN:
                        throw new InvalidOperationException("The token is not an impersonation token.");
                    default:
                        throw new System.ComponentModel.Win32Exception(err);
                }
            }
            return accessGranted;
        }

        /// <summary>
        /// Determines whether the user represented by the specified token handle 
        /// has been granted access to this securable object.
        /// </summary>
        /// <param name="tokenHandle">The handle to the token for which the 
        /// access is being verified. This must be an impersonation token.</param>
        /// <param name="desiredAccess">The desired access.</param>
        /// <returns>
        /// 	<c>true</c> if the desired access is allowed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAccessAllowed(SafeHandle tokenHandle, int desiredAccess)
        {
            int accessGranted;
            bool accessAllowed;
            int privilegeLength = 4096;
            byte[] accessCheckBuffer = new byte[privilegeLength];

            GenericMapping mapping = GetGenericMapping();
            NativeMethods.MapGenericMask(ref desiredAccess, mapping);

            if (!NativeMethods.AccessCheck(base.GetSecurityDescriptorBinaryForm(), tokenHandle,
                desiredAccess, mapping, accessCheckBuffer, out privilegeLength, out accessGranted,
                out accessAllowed))
            {
                int err = Marshal.GetLastWin32Error();
                switch (err)
                {
                    case NativeConstants.ERROR_ACCESS_DENIED:
                        throw new UnauthorizedAccessException();
                    case NativeConstants.ERROR_NO_IMPERSONATION_TOKEN:
                        throw new InvalidOperationException("The token is not an impersonation token.");
                    default:
                        throw new System.ComponentModel.Win32Exception(err);
                }
            }
            return accessAllowed;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (m_Handle != null) m_Handle.Close();
        }

        #endregion
    }
}
