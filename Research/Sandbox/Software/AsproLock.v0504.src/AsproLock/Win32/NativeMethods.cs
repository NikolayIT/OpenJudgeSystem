using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

using Asprosys.Security.AccessControl;

namespace Asprosys.Win32
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseWindowStation(IntPtr hWinSta);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("kernel32.dll")]
        private static extern void RtlMoveMemory(IntPtr destination,
            IntPtr source, uint length);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentProcessId();
        
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle, IntPtr hTargetProcessHandle,
            out IntPtr lpTargetHandle, int dwDesiredAccess,
            int bInheritHandle, int dwOptions);

        public static IntPtr DuplicateHandle(IntPtr sourceHandle, int desiredAccess)
        {
            IntPtr processHandle = GetCurrentProcess();
            IntPtr newHandle;

            if (!DuplicateHandle(processHandle, sourceHandle, processHandle, out newHandle,
                    desiredAccess, 0, 0))
            {
                int err = Marshal.GetLastWin32Error();
                switch (err)
                {
                    case NativeConstants.ERROR_ACCESS_DENIED:
                        throw new UnauthorizedAccessException();
                    case NativeConstants.ERROR_INVALID_HANDLE:
                        throw new InvalidOperationException();
                    default:
                        throw new System.ComponentModel.Win32Exception(err);
                }
            }
            return newHandle;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenFileMapping(int dwDesiredAccess,int bInheritHandle,
            string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetProcessId(IntPtr processHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, 
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenThread(int dwDesiredAccess, 
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwThreadId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenJobObject(int dwDesiredAccess, 
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenEvent(int dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenMutex(int dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenSemaphore(int dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenWaitableTimer(int dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenWindowStation(string lpszWinSta,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwDesiredAccess);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenDesktop(string lpszDesktop,
            int dwFlags, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, 
            int dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(SafeHandle processHandle,
            int desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(IntPtr processHandle,
            int desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenThreadToken(SafeHandle threadHandle,
            int desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool openAsSelf,
            out IntPtr tokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void MapGenericMask(ref int AccessMask,
            [In] Asprosys.Security.AccessControl.GenericMapping mapping);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AccessCheck(byte[] pSecurityDescriptor,
            SafeHandle clientToken, int desiredAccess, 
            [In] Asprosys.Security.AccessControl.GenericMapping mapping,
            byte[] privilegeSet, out int privilegeSetLength, out int grantedAccess,
            [MarshalAs(UnmanagedType.Bool)] out bool accessStatus);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AccessCheckAndAuditAlarm(string subsystemName,
            IntPtr handleId, string objectTypeName, string objectName,
            byte[] securityDescriptor, int desiredAccess, 
            Asprosys.Security.AccessControl.GenericMapping mapping,
            [MarshalAs(UnmanagedType.Bool)] bool objectCreation, out int grantedAccess,
            [MarshalAs(UnmanagedType.Bool)] out bool accessStatus,
            [MarshalAs(UnmanagedType.Bool)] out bool pfGenerateOnClose);


        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyPrivateObjectSecurity(ref IntPtr ObjectDescriptor);

        public static bool ClosePrivateObjectSecurity(IntPtr objectDescriptor)
        {
            return DestroyPrivateObjectSecurity(ref objectDescriptor);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreatePrivateObjectSecurity(
          byte[] ParentDescriptor,
          byte[] CreatorDescriptor,
          out IntPtr NewDescriptor,
          [MarshalAs(UnmanagedType.Bool)] bool IsDirectoryObject,
          IntPtr Token,
          ref GenericMapping GenericMapping
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPrivateObjectSecurity(
          IntPtr ObjectDescriptor,
          int SecurityInformation,
          byte[] ResultantDescriptor,
          int DescriptorLength,
          ref int ReturnLength
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetPrivateObjectSecurity(
          int SecurityInformation,
          IntPtr ModificationDescriptor,
          ref IntPtr ObjectsSecurityDescriptor,
          ref GenericMapping GenericMapping,
          IntPtr Token
        );


    }
}
