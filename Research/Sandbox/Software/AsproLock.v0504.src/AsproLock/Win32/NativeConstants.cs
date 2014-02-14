using System;

namespace Asprosys.Win32
{
    internal static class NativeConstants
    {
        public const int FILE_SHARE_READ = 1; 
        public const int FILE_SHARE_WRITE = 2;
        public const int FILE_SHARE_DELETE = 4;
        public const int FILE_SHARE_ALL = FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE;
        public const int FILE_ATTRIBUTE_NORMAL = 128;
        public const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000; //Open directory with CreateFile
        public const int OPEN_EXISTING = 3;
        public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

        public const int ACCESS_SYSTEM_SECURITY = 0x01000000;

        public const int BOOL_FAIL = 0;

        public const int ERROR_SUCCESS = 0;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_PATH_NOT_FOUND = 3;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_INVALID_HANDLE = 6;

        public const int ERROR_INSUFFICIENT_BUFFER = 122;

        public const int ERROR_NO_IMPERSONATION_TOKEN = 1309;
        public const int ERROR_NO_SUCH_PRIVILEGE = 1313;
    }
}
