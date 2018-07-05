namespace OJS.Workers.Common.AppSettings
{
    public static partial class Settings
    {
        public static int NodeJsBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("NodeJsBaseTimeUsedInMilliseconds", 0);

        public static int NodeJsBaseMemoryUsedInBytes =>
            GetSettingOrDefault("NodeJsBaseMemoryUsedInBytes", 0);

        public static int MsBuildBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("MsBuildBaseTimeUsedInMilliseconds", 0);

        public static int MsBuildBaseMemoryUsedInBytes =>
            GetSettingOrDefault("MsBuildBaseMemoryUsedInBytes", 0);

        public static int DotNetCscBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("DotNetCscBaseTimeUsedInMilliseconds", 0);

        public static int DotNetCscBaseMemoryUsedInBytes =>
            GetSettingOrDefault("DotNetCscBaseMemoryUsedInBytes", 0);

        public static int DotNetCliBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("DotNetCliBaseTimeUsedInMilliseconds", 0);

        public static int DotNetCliBaseMemoryUsedInBytes =>
            GetSettingOrDefault("DotNetCliBaseMemoryUsedInBytes", 0);

        public static int JavaBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("JavaBaseTimeUsedInMilliseconds", 0);

        public static int JavaBaseMemoryUsedInBytes =>
            GetSettingOrDefault("JavaBaseMemoryUsedInBytes", 0);

        public static int JavaBaseUpdateTimeOffsetInMilliseconds =>
            GetSettingOrDefault("JavaBaseUpdateTimeOffsetInMilliseconds", 0);

        public static int GPlusPlusBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("GPlusPlusBaseTimeUsedInMilliseconds", 0);

        public static int GPlusPlusBaseMemoryUsedInBytes =>
            GetSettingOrDefault("GPlusPlusBaseMemoryUsedInBytes", 0);

        public static int PhpCgiBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("PhpCgiBaseTimeUsedInMilliseconds", 0);

        public static int PhpCgiBaseMemoryUsedInBytes =>
            GetSettingOrDefault("PhpCgiBaseMemoryUsedInBytes", 0);

        public static int PhpCliBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("PhpCliBaseTimeUsedInMilliseconds", 0);

        public static int PhpCliBaseMemoryUsedInBytes =>
            GetSettingOrDefault("PhpCliBaseMemoryUsedInBytes", 0);

        public static int RubyBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("RubyBaseTimeUsedInMilliseconds", 0);

        public static int RubyBaseMemoryUsedInBytes =>
            GetSettingOrDefault("RubyBaseMemoryUsedInBytes", 0);

        public static int PythonBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("PythonBaseTimeUsedInMilliseconds", 0);

        public static int PythonBaseMemoryUsedInBytes =>
            GetSettingOrDefault("PythonBaseMemoryUsedInBytes", 0);
    }
}