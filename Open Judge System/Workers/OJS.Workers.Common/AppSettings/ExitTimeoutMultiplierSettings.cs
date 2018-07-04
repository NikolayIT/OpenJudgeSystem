namespace OJS.Workers.Common.AppSettings
{
    public static partial class Settings
    {
        public static int CPlusPlusCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("CPlusPlusCompilerProcessExitTimeOutMultiplier", 1);

        public static int CPlusPlusZipCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("CPlusPlusZipCompilerProcessExitTimeOutMultiplier", 1);

        public static int CSharpCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("CSharpCompilerProcessExitTimeOutMultiplier", 1);

        public static int CSharpDotNetCoreCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("CSharpDotNetCoreCompilerProcessExitTimeOutMultiplier", 1);

        public static int DotNetCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("DotNetCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("JavaCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaInPlaceCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("JavaInPlaceCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaZipCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("JavaZipCompilerProcessExitTimeOutMultiplier", 1);

        public static int MsBuildCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("MsBuildCompilerProcessExitTimeOutMultiplier", 1);

        public static int MsBuildLibraryCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("MsBuildLibraryCompilerProcessExitTimeOutMultiplier", 1);
    }
}