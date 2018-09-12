namespace OJS.Workers.Common.Helpers
{
    using System;
    using System.Configuration;

    public static class SettingsHelper
    {
        public static string GetSetting(string settingName)
        {
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                throw new Exception($"{settingName} setting not found in App.config file!");
            }

            return ConfigurationManager.AppSettings[settingName];
        }

        public static T GetSettingOrDefault<T>(string settingName, T defaultValue)
        {
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(ConfigurationManager.AppSettings[settingName], typeof(T));
        }
    }
}