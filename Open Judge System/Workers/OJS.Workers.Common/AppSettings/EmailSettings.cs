namespace OJS.Workers.Common.AppSettings
{
    public static partial class Settings
    {
        public static string EmailServerHost => GetSetting("EmailServerHost");

        public static int EmailServerPort => GetSettingOrDefault("EmailServerPort", 25);

        public static string EmailServerUsername => GetSetting("EmailServerUsername");

        public static string EmailServerPassword => GetSetting("EmailServerPassword");

        public static string EmailSenderEmail => GetSetting("EmailSenderEmail");

        public static string EmailSenderDisplayName => GetSetting("EmailSenderDisplayName");

        public static string DevEmail => GetSetting("DevEmail");
    }
}