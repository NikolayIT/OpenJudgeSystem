namespace OJS.Workers.Common.Communication
{
    using System;

    [Serializable]
    public class SystemInformation
    {
        public bool Is64BitOperatingSystem { get; set; }

        public static SystemInformation Collect()
        {
            var information = new SystemInformation { Is64BitOperatingSystem = Environment.Is64BitOperatingSystem };
            return information;
        }
    }
}
