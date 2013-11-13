using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSubmissionResponder
{
    using System.Configuration;

    using log4net;

    public static class Settings
    {
        private static readonly ILog logger;

        static Settings()
        {
            logger = LogManager.GetLogger("Settings");
        }

        public static string CSharpCompilerPath
        {
            get
            {
                return GetSetting("CSharpCompilerPath");
            }
        }

        public static string CPlusPlusGccCompilerPath
        {
            get
            {
                return GetSetting("CPlusPlusGccCompilerPath");
            }
        }

        public static string NodeJsExecutablePath
        {
            get
            {
                return GetSetting("NodeJsExecutablePath");
            }
        }

        private static string GetSetting(string settingName)
        {
            if (ConfigurationManager.AppSettings[settingName] == null)
            {
                logger.FatalFormat("{0} setting not found in App.config file!", settingName);
                throw new Exception(string.Format("{0} setting not found in App.config file!", settingName));
            }

            return ConfigurationManager.AppSettings[settingName];
        }
    }
}
