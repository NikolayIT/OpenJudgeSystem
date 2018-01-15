namespace OJS.Web
{
    using System;
    using System.Configuration;

    public static class Settings
    {
        public static string ApiKey => GetSetting("ApiKey");

        public static string CSharpCompilerPath => GetSetting("CSharpCompilerPath");

        public static string DotNetDisassemblerPath => GetSetting("DotNetDisassemblerPath");

        public static string JavaCompilerPath => GetSetting("JavaCompilerPath");

        public static string JavaDisassemblerPath => GetSetting("JavaDisassemblerPath");

        public static string SulsPlatformBaseUrl => GetSetting("SulsPlatformBaseUrl");

        public static string SvnBaseUrl => GetSetting("SvnBaseUrl");

        public static string GetExternalUserUrl => GetSetting("GetExternalUserUrl");

        public static string CanUserCompeteInContestUrl => GetSetting("CanUserCompeteInContestUrl");

        public static string LearningSystemSvnDownloadBaseUrl => GetSetting("LearningSystemSvnDownloadBaseUrl");

        private static string GetSetting(string settingName)
        {
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                throw new Exception($"{settingName} setting not found in App.config file!");
            }

            return ConfigurationManager.AppSettings[settingName];
        }
    }
}