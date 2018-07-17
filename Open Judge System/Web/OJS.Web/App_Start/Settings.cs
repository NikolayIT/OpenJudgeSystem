namespace OJS.Web
{
    using static OJS.Common.Helpers.SettingsHelper;

    public static class Settings
    {
        public static string ApiKey => GetSetting("ApiKey");

        public static string CSharpCompilerPath => GetSetting("CSharpCompilerPath");

        public static string DotNetDisassemblerPath => GetSetting("DotNetDisassemblerPath");

        public static string JavaCompilerPath => GetSetting("JavaCompilerPath");

        public static string JavaDisassemblerPath => GetSetting("JavaDisassemblerPath");

        public static string SulsPlatformBaseUrl => GetSetting("SulsPlatformBaseUrl");

        public static string SvnBaseUrl => GetSetting("SvnBaseUrl");

        public static string LearningSystemSvnDownloadBaseUrl => GetSetting("LearningSystemSvnDownloadBaseUrl");

        public static int CSharpCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("CSharpCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("JavaCompilerProcessExitTimeOutMultiplier", 1);
    }
}