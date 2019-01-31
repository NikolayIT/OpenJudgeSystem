namespace OJS.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    public class EncryptionHelper
    {
        private const string Provider = "RsaProtectedConfigurationProvider";

        public static void EncryptAppConfigSections(params string[] sectionNames)
        {
            var config = GetExeConfiguration();

            ProtectConfigurationSections(config, GetSections(config, sectionNames));
        }

        public static void DecryptAppConfigSections(params string[] sectionNames)
        {
            var config = GetExeConfiguration();

            UnprotectConfigurationSections(config, GetSections(config, sectionNames));
        }

        private static Configuration GetExeConfiguration() =>
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        private static IEnumerable<ConfigurationSection> GetSections(
            Configuration config,
            params string[] sectionNames) =>
                sectionNames.Select(config.GetSection).ToList();

        private static void ProtectConfigurationSections(
            Configuration configuration,
            IEnumerable<ConfigurationSection> sections)
        {
            foreach (var section in sections)
            {
                if (IsSectionValid(section, sectionShouldBeEncrypted: true))
                {
                    section.SectionInformation.ProtectSection(Provider);

                    SaveSectionInConfiguration(section, configuration);
                }
            }
        }

        private static void UnprotectConfigurationSections(
            Configuration configuration,
            IEnumerable<ConfigurationSection> sections)
        {
            foreach (var section in sections)
            {
                if (IsSectionValid(section, sectionShouldBeEncrypted: false))
                {
                    section.SectionInformation.UnprotectSection();

                    SaveSectionInConfiguration(section, configuration);
                }
            }
        }

        private static bool IsSectionValid(ConfigurationSection section, bool sectionShouldBeEncrypted)
        {
            if (section == null)
            {
                return false;
            }

            var sectionIsEncrypted = section.SectionInformation.IsProtected;
            var sectionShouldBeDecrypted = !sectionShouldBeEncrypted;
            var sectionIsDecrypted = !sectionIsEncrypted;

            if ((sectionShouldBeEncrypted && sectionIsEncrypted) ||
                (sectionShouldBeDecrypted && sectionIsDecrypted))
            {
                return false;
            }

            if (section.ElementInformation.IsLocked)
            {
                throw new InvalidOperationException($"{section.SectionInformation.Name} is locked!");
            }

            return true;
        }

        private static void SaveSectionInConfiguration(ConfigurationSection section, Configuration configuration)
        {
            section.SectionInformation.ForceSave = true;

            configuration.Save(ConfigurationSaveMode.Full);
        }
    }
}
