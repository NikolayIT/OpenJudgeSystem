namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    using Microsoft.Win32.SafeHandles;

    public static class NativeMethods
    {
        public const int SYNCHRONIZE = 0x00100000;
        public const int PROCESS_TERMINATE = 0x0001;
        public const int STILL_ACTIVE = 0x00000103;

        public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const uint STANDARD_RIGHTS_READ = 0x00020000;

        public const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        public const uint TOKEN_DUPLICATE = 0x0002;
        public const uint TOKEN_IMPERSONATE = 0x0004;
        public const uint TOKEN_QUERY = 0x0008;
        public const uint TOKEN_QUERY_SOURCE = 0x0010;
        public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        public const uint TOKEN_ADJUST_GROUPS = 0x0040;
        public const uint TOKEN_ADJUST_DEFAULT = 0x0080;
        public const uint TOKEN_ADJUST_SESSIONID = 0x0100;
        public const uint TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;
        public const uint TOKEN_ALL_ACCESS =
            STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY
             | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT
             | TOKEN_ADJUST_SESSIONID;

        // Group related SID Attributes
        public const uint SE_GROUP_MANDATORY = 0x00000001;
        public const uint SE_GROUP_ENABLED_BY_DEFAULT = 0x00000002;
        public const uint SE_GROUP_ENABLED = 0x00000004;
        public const uint SE_GROUP_OWNER = 0x00000008;
        public const uint SE_GROUP_USE_FOR_DENY_ONLY = 0x00000010;
        public const uint SE_GROUP_INTEGRITY = 0x00000020;
        public const uint SE_GROUP_INTEGRITY_ENABLED = 0x00000040;
        public const uint SE_GROUP_LOGON_ID = 0xC0000000;
        public const uint SE_GROUP_RESOURCE = 0x20000000;
        public const uint SE_GROUP_VALID_ATTRIBUTES = SE_GROUP_MANDATORY |
            SE_GROUP_ENABLED_BY_DEFAULT | SE_GROUP_ENABLED | SE_GROUP_OWNER |
            SE_GROUP_USE_FOR_DENY_ONLY | SE_GROUP_LOGON_ID | SE_GROUP_RESOURCE |
            SE_GROUP_INTEGRITY | SE_GROUP_INTEGRITY_ENABLED;

        public const int SAFER_SCOPEID_MACHINE = 1;
        public const int SAFER_SCOPEID_USER = 2;

        public const int SAFER_LEVELID_DISALLOWED = 0x00000;
        public const int SAFER_LEVELID_UNTRUSTED = 0x1000;
        public const int SAFER_LEVELID_CONSTRAINED = 0x10000;
        public const int SAFER_LEVELID_NORMALUSER = 0x20000;
        public const int SAFER_LEVELID_FULLYTRUSTED = 0x40000;

        public const int SAFER_LEVEL_OPEN = 1;

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static SidIdentifierAuthority SECURITY_MANDATORY_LABEL_AUTHORITY =
            new SidIdentifierAuthority(new byte[] { 0, 0, 0, 0, 0, 16 });

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Security.SuppressUnmanagedCodeSecurityAttribute]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            SecurityAttributes lpProcessAttributes,
            SecurityAttributes lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInformation);

        [DllImport("kernel32.dll")]
        internal static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.Process)]
        internal static extern bool CreatePipe(
            out SafeFileHandle hReadPipe,
            out SafeFileHandle hWritePipe,
            SecurityAttributes lpPipeAttributes,
            int nSize);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool DuplicateHandle(
            HandleRef hSourceProcessHandle,
            SafeHandle hSourceHandle,
            HandleRef hTargetProcess,
            out SafeFileHandle targetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool DuplicateHandle(
            HandleRef hSourceProcessHandle,
            SafeHandle hSourceHandle,
            HandleRef hTargetProcess,
            out SafeWaitHandle targetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle,
            IntPtr hTargetProcessHandle,
            out IntPtr lpTargetHandle,
            uint dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            uint dwOptions);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool TerminateProcess(SafeProcessHandle processHandle, int exitCode);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetExitCodeProcess(SafeProcessHandle processHandle, out int exitCode);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetProcessTimes(
            SafeProcessHandle handle,
            out long creation,
            out long exit,
            out long kernel,
            out long user);

        /// <summary>
        /// The function opens the access token associated with a process.
        /// </summary>
        /// <param name="processHandle">A handle to the process whose access token is opened.</param>
        /// <param name="desiredAccess">Specifies an access mask that specifies the requested types of access to the access token.</param>
        /// <param name="tokenHandle">Outputs a handle that identifies the newly opened access token when the function returns.</param>
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool CreateRestrictedToken(
            IntPtr existingTokenHandle,
            CreateRestrictedTokenFlags createRestrictedTokenFlags,
            int disableSidCount,
            SidAndAttributes[] sidsToDisable,
            int deletePrivilegeCount,
            LuidAndAttributes[] privilegesToDelete,
            int restrictedSidCount,
            SidAndAttributes[] sidsToRestrict,
            out IntPtr newTokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool ConvertStringSidToSid(string StringSid, out IntPtr ptrSid);

        /// <summary>
        /// The function sets various types of information for a specified
        /// access token. The information that this function sets replaces
        /// existing information. The calling process must have appropriate
        /// access rights to set the information.
        /// </summary>
        /// <param name="hToken"> A handle to the access token for which information is to be set.</param>
        /// <param name="tokenInfoClass">A value from the TOKEN_INFORMATION_CLASS enumerated type that identifies the type of information the function sets.</param>
        /// <param name="pTokenInfo">A pointer to a buffer that contains the information set in the access token.</param>
        /// <param name="tokenInfoLength">Specifies the length, in bytes, of the buffer pointed to by TokenInformation.</param>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetTokenInformation(
            IntPtr hToken,
            TokenInformationClass tokenInfoClass,
            IntPtr pTokenInfo,
            int tokenInfoLength);

        /// <summary>
        /// The function returns the length, in bytes, of a valid security
        /// identifier (SID).
        /// </summary>
        /// <param name="pSid">A pointer to the SID structure whose length is returned.</param>
        /// <returns>
        /// If the SID structure is valid, the return value is the length, in
        /// bytes, of the SID structure.
        /// </returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetLengthSid(IntPtr pSid);

        /// <summary>
        /// The AllocateAndInitializeSid function allocates and initializes a
        /// security identifier (SID) with up to eight subauthorities.
        /// </summary>
        /// <param name="pIdentifierAuthority">A reference of a SID_IDENTIFIER_AUTHORITY structure. This structure provides the top-level identifier authority value to set in the SID.</param>
        /// <param name="nSubAuthorityCount">Specifies the number of subauthorities to place in the SID.</param>
        /// <param name="dwSubAuthority0">Subauthority value to place in the SID.</param>
        /// <param name="dwSubAuthority1">Subauthority value to place in the SID.</param>
        /// <param name="dwSubAuthority2">Subauthority value to place in the SID.</param>
        /// <param name="dwSubAuthority3">Subauthority value to place in the SID.</param>
        /// <param name="dwSubAuthority4">Subauthority value to place in the SID.</param>
        /// <param name="dwSubAuthority5">Subauthority value to place in the SID.</param>
        /// <param name="dwSubAuthority6">Subauthority value to place in the SID.</param>
        /// <param name="dwSubAuthority7">Subauthority value to place in the SID.</param>
        /// <param name="pSid">Outputs the allocated and initialized SID structure.</param>
        /// <returns>
        /// If the function succeeds, the return value is true. If the
        /// function fails, the return value is false. To get extended error
        /// information, call GetLastError.
        /// </returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AllocateAndInitializeSid(
            ref SidIdentifierAuthority pIdentifierAuthority,
            byte nSubAuthorityCount,
            int dwSubAuthority0,
            int dwSubAuthority1,
            int dwSubAuthority2,
            int dwSubAuthority3,
            int dwSubAuthority4,
            int dwSubAuthority5,
            int dwSubAuthority6,
            int dwSubAuthority7,
            out IntPtr pSid);

        [DllImport("ntdll.dll", CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int NtQuerySystemInformation(int query, IntPtr dataPtr, int size, out int returnedSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeProcessHandle OpenProcess(int access, bool inherit, int processId);

        [DllImport("psapi.dll", EntryPoint = "GetProcessMemoryInfo")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetProcessMemoryInfo([In] IntPtr Process, [Out] out ProcessMemoryCounters ppsmemCounters, uint cb);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool SaferCloseLevel(IntPtr hLevelHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool SaferCreateLevel(int dwScopeId, int dwLevelId, int OpenFlags, out IntPtr pLevelHandle, IntPtr lpReserved);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool SaferComputeTokenFromLevel(IntPtr LevelHandle, IntPtr InAccessToken, out IntPtr OutAccessToken, int dwFlags, IntPtr lpReserved);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        // SafeLocalMemHandle
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        internal static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string stringSecurityDescriptor, int stringSDRevision, out SafeLocalMemHandle securityDescriptor, IntPtr securityDescriptorSize);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll")]
        internal static extern IntPtr LocalFree(IntPtr memoryHandler);
    }
}
