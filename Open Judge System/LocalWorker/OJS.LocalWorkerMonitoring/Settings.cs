namespace OJS.LocalWorkerMonitoring
{
    using static OJS.Workers.Common.Helpers.SettingsHelper;

    internal static class Settings
    {
        public static string LocalWorkerServiceExecutablePath => GetSetting("LocalWorkerServiceExecutablePath");

        public static string EmailServerHost => GetSetting("EmailServerHost");

        public static int EmailServerPort => GetSettingOrDefault("EmailServerPort", 25);

        public static string EmailServerUsername => GetSetting("EmailServerUsername");

        public static string EmailServerPassword => GetSetting("EmailServerPassword");

        public static string EmailSenderEmail => GetSetting("EmailSenderEmail");

        public static string EmailSenderDisplayName => GetSetting("EmailSenderDisplayName");

        public static string DevEmail => GetSetting("DevEmail");
    }
}