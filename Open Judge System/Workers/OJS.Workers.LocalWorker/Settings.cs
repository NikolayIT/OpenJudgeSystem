namespace OJS.Workers.LocalWorker
{
    using System;
    using System.Configuration;

    using log4net;

    public static class Settings
    {
        private static readonly ILog Logger;

        static Settings()
        {
            Logger = LogManager.GetLogger("Settings");
        }

        public static string CSharpCompilerPath => GetSetting("CSharpCompilerPath");

        public static string CPlusPlusGccCompilerPath => GetSetting("CPlusPlusGccCompilerPath");

        public static string MsBuildExecutablePath => GetSetting("MsBuildExecutablePath");

        public static string NuGetExecutablePath => GetSetting("NuGetExecutablePath");

        public static string JavaCompilerPath => GetSetting("JavaCompilerPath");

        public static string JavaExecutablePath => GetSetting("JavaExecutablePath");

        public static string NodeJsExecutablePath => GetSetting("NodeJsExecutablePath");

        public static string MochaModulePath => GetSetting("MochaModulePath");

        public static string ChaiModulePath => GetSetting("ChaiModulePath");

        public static string IoJsExecutablePath => GetSetting("IoJsExecutablePath");

        public static string JsDomModulePath => GetSetting("JsDomModulePath");

        public static string JQueryModulePath => GetSetting("JQueryModulePath");

        public static string HandlebarsModulePath => GetSetting("HandlebarsModulePath");

        public static string SinonModulePath => GetSetting("SinonModulePath");

        public static string SinonChaiModulePath => GetSetting("SinonChaiModulePath");

        public static string UnderscoreModulePath => GetSetting("UnderscoreModulePath");

        public static string PythonExecutablePath => GetSetting("PythonExecutablePath");

        public static string PhpCgiExecutablePath => GetSetting("PhpCgiExecutablePath");

        public static string PhpCliExecutablePath => GetSetting("PhpCliExecutablePath");

        public static int ThreadsCount => GetSettingOrDefault("ThreadsCount", 2);

        private static string GetSetting(string settingName)
        {
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                Logger.FatalFormat("{0} setting not found in App.config file!", settingName);
                throw new Exception($"{settingName} setting not found in App.config file!");
            }

            return ConfigurationManager.AppSettings[settingName];
        }

        private static T GetSettingOrDefault<T>(string settingName, T defaultValue)
        {
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(ConfigurationManager.AppSettings[settingName], typeof(T));
        }
    }
}
