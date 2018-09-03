namespace OJS.Workers.Common
{
    using OJS.Common.Helpers;

    public static class Settings
    {
        public static string MonitoringServiceExecutablePath => SettingsHelper.GetSetting("MonitoringServiceExecutablePath");

        public static string DotNetCompilerPath => SettingsHelper.GetSetting("DotNetCompilerPath");

        public static string MavenPath => SettingsHelper.GetSetting("MavenPath");

        public static string CSharpCompilerPath => SettingsHelper.GetSetting("CSharpCompilerPath");

        public static string CSharpDotNetCoreCompilerPath => SettingsHelper.GetSetting("CSharpDotNetCoreCompilerPath");

        public static string DotNetCoreSharedAssembliesPath => SettingsHelper.GetSetting("DotNetCoreSharedAssembliesPath");

        public static string CPlusPlusGccCompilerPath => SettingsHelper.GetSetting("CPlusPlusGccCompilerPath");

        public static string NUnitConsoleRunnerPath => SettingsHelper.GetSetting("NUnitConsoleRunnerPath");

        public static string MsBuildExecutablePath => SettingsHelper.GetSetting("MsBuildExecutablePath");

        public static string NuGetExecutablePath => SettingsHelper.GetSetting("NuGetExecutablePath");

        public static string JavaCompilerPath => SettingsHelper.GetSetting("JavaCompilerPath");

        public static string JavaExecutablePath => SettingsHelper.GetSetting("JavaExecutablePath");

        public static string JavaLibsPath => SettingsHelper.GetSetting("JavaLibsPath");

        public static string RubyPath => SettingsHelper.GetSetting("RubyPath");

        public static string NodeJsExecutablePath => SettingsHelper.GetSetting("NodeJsExecutablePath");

        public static string MochaModulePath => SettingsHelper.GetSetting("MochaModulePath");

        public static string ChaiModulePath => SettingsHelper.GetSetting("ChaiModulePath");

        public static string JsDomModulePath => SettingsHelper.GetSetting("JsDomModulePath");

        public static string JQueryModulePath => SettingsHelper.GetSetting("JQueryModulePath");

        public static string HandlebarsModulePath => SettingsHelper.GetSetting("HandlebarsModulePath");

        public static string SinonModulePath => SettingsHelper.GetSetting("SinonModulePath");

        public static string SinonJsDomModulePath => SettingsHelper.GetSetting("SinonJsDomModulePath");

        public static string SinonChaiModulePath => SettingsHelper.GetSetting("SinonChaiModulePath");

        public static string UnderscoreModulePath => SettingsHelper.GetSetting("UnderscoreModulePath");

        public static string BrowserifyModulePath => SettingsHelper.GetSetting("BrowserifyModulePath");

        public static string BabelifyModulePath => SettingsHelper.GetSetting("BabelifyModulePath");

        public static string Es2015ImportPluginPath => SettingsHelper.GetSetting("ES2015ImportPluginPath");

        public static string BabelCoreModulePath => SettingsHelper.GetSetting("BabelCoreModulePath");

        public static string ReactJsxPluginPath => SettingsHelper.GetSetting("ReactJsxPluginPath");

        public static string ReactModulePath => SettingsHelper.GetSetting("ReactModulePath");

        public static string ReactDomModulePath => SettingsHelper.GetSetting("ReactDOMModulePath");

        public static string BootstrapModulePath => SettingsHelper.GetSetting("BootstrapModulePath");

        public static string BootstrapCssPath => SettingsHelper.GetSetting("BootstrapCssPath");

        public static string PythonExecutablePath => SettingsHelper.GetSetting("PythonExecutablePath");

        public static string PhpCgiExecutablePath => SettingsHelper.GetSetting("PhpCgiExecutablePath");

        public static string PhpCliExecutablePath => SettingsHelper.GetSetting("PhpCliExecutablePath");

        public static string SqlServerLocalDbMasterDbConnectionString =>
            SettingsHelper.GetSetting("SqlServerLocalDbMasterDbConnectionString");

        public static string SqlServerLocalDbRestrictedUserId =>
            SettingsHelper.GetSetting("SqlServerLocalDbRestrictedUserId");

        public static string SqlServerLocalDbRestrictedUserPassword =>
            SettingsHelper.GetSetting("SqlServerLocalDbRestrictedUserPassword");

        public static string MySqlSysDbConnectionString => SettingsHelper.GetSetting("MySqlSysDbConnectionString");

        public static string MySqlRestrictedUserId => SettingsHelper.GetSetting("MySqlRestrictedUserId");

        public static string MySqlRestrictedUserPassword => SettingsHelper.GetSetting("MySqlRestrictedUserPassword");

        public static int ThreadsCount => SettingsHelper.GetSettingOrDefault("ThreadsCount", 2);

        // Base time and memory used
        public static int NodeJsBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("NodeJsBaseTimeUsedInMilliseconds", 0);

        public static int NodeJsBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("NodeJsBaseMemoryUsedInBytes", 0);

        public static int MsBuildBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("MsBuildBaseTimeUsedInMilliseconds", 0);

        public static int MsBuildBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("MsBuildBaseMemoryUsedInBytes", 0);

        public static int DotNetCscBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("DotNetCscBaseTimeUsedInMilliseconds", 0);

        public static int DotNetCscBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("DotNetCscBaseMemoryUsedInBytes", 0);

        public static int DotNetCliBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("DotNetCliBaseTimeUsedInMilliseconds", 0);

        public static int DotNetCliBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("DotNetCliBaseMemoryUsedInBytes", 0);

        public static int JavaBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("JavaBaseTimeUsedInMilliseconds", 0);

        public static int JavaBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("JavaBaseMemoryUsedInBytes", 0);

        public static int JavaBaseUpdateTimeOffsetInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("JavaBaseUpdateTimeOffsetInMilliseconds", 0);

        public static int GPlusPlusBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("GPlusPlusBaseTimeUsedInMilliseconds", 0);

        public static int GPlusPlusBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("GPlusPlusBaseMemoryUsedInBytes", 0);

        public static int PhpCgiBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("PhpCgiBaseTimeUsedInMilliseconds", 0);

        public static int PhpCgiBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("PhpCgiBaseMemoryUsedInBytes", 0);

        public static int PhpCliBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("PhpCliBaseTimeUsedInMilliseconds", 0);

        public static int PhpCliBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("PhpCliBaseMemoryUsedInBytes", 0);

        public static int RubyBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("RubyBaseTimeUsedInMilliseconds", 0);

        public static int RubyBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("RubyBaseMemoryUsedInBytes", 0);

        public static int PythonBaseTimeUsedInMilliseconds =>
            SettingsHelper.GetSettingOrDefault("PythonBaseTimeUsedInMilliseconds", 0);

        public static int PythonBaseMemoryUsedInBytes =>
            SettingsHelper.GetSettingOrDefault("PythonBaseMemoryUsedInBytes", 0);

        // Compiler time out multipliers
        public static int CPlusPlusCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("CPlusPlusCompilerProcessExitTimeOutMultiplier", 1);

        public static int CPlusPlusZipCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("CPlusPlusZipCompilerProcessExitTimeOutMultiplier", 1);

        public static int CSharpCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("CSharpCompilerProcessExitTimeOutMultiplier", 1);

        public static int CSharpDotNetCoreCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("CSharpDotNetCoreCompilerProcessExitTimeOutMultiplier", 1);

        public static int DotNetCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("DotNetCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("JavaCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaInPlaceCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("JavaInPlaceCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaZipCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("JavaZipCompilerProcessExitTimeOutMultiplier", 1);

        public static int MsBuildCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("MsBuildCompilerProcessExitTimeOutMultiplier", 1);

        public static int MsBuildLibraryCompilerProcessExitTimeOutMultiplier =>
            SettingsHelper.GetSettingOrDefault("MsBuildLibraryCompilerProcessExitTimeOutMultiplier", 1);
    }
}