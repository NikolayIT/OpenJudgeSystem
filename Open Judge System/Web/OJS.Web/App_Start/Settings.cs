namespace OJS.Web
{
    using System;
    using System.Configuration;

    public static class Settings
    {
        public static string CSharpCompilerPath
        {
            get
            {
                return GetSetting("CSharpCompilerPath");
            }
        }

        public static string DotNetDisassemblerPath
        {
            get
            {
                return GetSetting("DotNetDisassemblerPath");
            }
        }

        private static string GetSetting(string settingName)
        {
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                throw new Exception(string.Format("{0} setting not found in App.config file!", settingName));
            }

            return ConfigurationManager.AppSettings[settingName];
        }
    }
}