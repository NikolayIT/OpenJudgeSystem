namespace OJS.Workers.LocalWorker
{
    using static OJS.Common.Helpers.SettingsHelper;

    internal static class Settings
    {
        public static string MonitoringServiceExecutablePath => GetSetting("MonitoringServiceExecutablePath");

        public static string DotNetCompilerPath => GetSetting("DotNetCompilerPath");

        public static string SolidityCompilerPath => GetSetting("SolidityCompilerPath");

        public static string MavenPath => GetSetting("MavenPath");

        public static string CSharpCompilerPath => GetSetting("CSharpCompilerPath");

        public static string CSharpDotNetCoreCompilerPath => GetSetting("CSharpDotNetCoreCompilerPath");

        public static string DotNetCoreSharedAssembliesPath => GetSetting("DotNetCoreSharedAssembliesPath");

        public static string CPlusPlusGccCompilerPath => GetSetting("CPlusPlusGccCompilerPath");

        public static string NUnitConsoleRunnerPath => GetSetting("NUnitConsoleRunnerPath");

        public static string MsBuildExecutablePath => GetSetting("MsBuildExecutablePath");

        public static string NuGetExecutablePath => GetSetting("NuGetExecutablePath");

        public static string JavaCompilerPath => GetSetting("JavaCompilerPath");

        public static string JavaExecutablePath => GetSetting("JavaExecutablePath");

        public static string JavaLibsPath => GetSetting("JavaLibsPath");

        public static string RubyPath => GetSetting("RubyPath");

        public static string NodeJsExecutablePath => GetSetting("NodeJsExecutablePath");

        public static string MochaModulePath => GetSetting("MochaModulePath");

        public static string ChaiModulePath => GetSetting("ChaiModulePath");

        public static string JsDomModulePath => GetSetting("JsDomModulePath");

        public static string JQueryModulePath => GetSetting("JQueryModulePath");

        public static string HandlebarsModulePath => GetSetting("HandlebarsModulePath");

        public static string SinonModulePath => GetSetting("SinonModulePath");

        public static string SinonJsDomModulePath => GetSetting("SinonJsDomModulePath");

        public static string SinonChaiModulePath => GetSetting("SinonChaiModulePath");

        public static string UnderscoreModulePath => GetSetting("UnderscoreModulePath");

        public static string BrowserifyModulePath => GetSetting("BrowserifyModulePath");

        public static string BabelifyModulePath => GetSetting("BabelifyModulePath");

        public static string Es2015ImportPluginPath => GetSetting("ES2015ImportPluginPath");

        public static string BabelCoreModulePath => GetSetting("BabelCoreModulePath");

        public static string ReactJsxPluginPath => GetSetting("ReactJsxPluginPath");

        public static string ReactModulePath => GetSetting("ReactModulePath");

        public static string ReactDomModulePath => GetSetting("ReactDOMModulePath");

        public static string BootstrapModulePath => GetSetting("BootstrapModulePath");

        public static string BootstrapCssPath => GetSetting("BootstrapCssPath");

        public static string PythonExecutablePath => GetSetting("PythonExecutablePath");

        public static string PhpCgiExecutablePath => GetSetting("PhpCgiExecutablePath");

        public static string PhpCliExecutablePath => GetSetting("PhpCliExecutablePath");

        public static string SqlServerLocalDbMasterDbConnectionString =>
            GetSetting("SqlServerLocalDbMasterDbConnectionString");

        public static string GanacheCliNodeExecutablePath => GetSetting("GanacheCliNodeExecutablePath");

        public static string TruffleCliNodeExecutablePath => GetSetting("TruffleCliNodeExecutablePath");

        public static string SqlServerLocalDbRestrictedUserId =>
            GetSetting("SqlServerLocalDbRestrictedUserId");

        public static string SqlServerLocalDbRestrictedUserPassword =>
            GetSetting("SqlServerLocalDbRestrictedUserPassword");

        public static string MySqlSysDbConnectionString => GetSetting("MySqlSysDbConnectionString");

        public static string MySqlRestrictedUserId => GetSetting("MySqlRestrictedUserId");

        public static string MySqlRestrictedUserPassword => GetSetting("MySqlRestrictedUserPassword");

        public static int ThreadsCount => GetSettingOrDefault("ThreadsCount", 2);

        public static int GanacheCliDefaultPort => GetSettingOrDefault("GanacheCliDefaultPort", 8545);

        // Base time/memory used by processes
        public static int NodeJsBaseTimeUsedInMilliseconds => GetSettingOrDefault("NodeJsBaseTimeUsedInMilliseconds", 0);

        public static int NodeJsBaseMemoryUsedInBytes =>
            GetSettingOrDefault("NodeJsBaseMemoryUsedInBytes", 0);

        public static int MsBuildBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("MsBuildBaseTimeUsedInMilliseconds", 0);

        public static int MsBuildBaseMemoryUsedInBytes =>
            GetSettingOrDefault("MsBuildBaseMemoryUsedInBytes", 0);

        public static int DotNetCscBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("DotNetCscBaseTimeUsedInMilliseconds", 0);

        public static int DotNetCscBaseMemoryUsedInBytes =>
            GetSettingOrDefault("DotNetCscBaseMemoryUsedInBytes", 0);

        public static int DotNetCliBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("DotNetCliBaseTimeUsedInMilliseconds", 0);

        public static int DotNetCliBaseMemoryUsedInBytes =>
            GetSettingOrDefault("DotNetCliBaseMemoryUsedInBytes", 0);

        public static int JavaBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("JavaBaseTimeUsedInMilliseconds", 0);

        public static int JavaBaseMemoryUsedInBytes =>
            GetSettingOrDefault("JavaBaseMemoryUsedInBytes", 0);

        public static int JavaBaseUpdateTimeOffsetInMilliseconds =>
            GetSettingOrDefault("JavaBaseUpdateTimeOffsetInMilliseconds", 0);

        public static int GPlusPlusBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("GPlusPlusBaseTimeUsedInMilliseconds", 0);

        public static int GPlusPlusBaseMemoryUsedInBytes =>
            GetSettingOrDefault("GPlusPlusBaseMemoryUsedInBytes", 0);

        public static int PhpCgiBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("PhpCgiBaseTimeUsedInMilliseconds", 0);

        public static int PhpCgiBaseMemoryUsedInBytes =>
            GetSettingOrDefault("PhpCgiBaseMemoryUsedInBytes", 0);

        public static int PhpCliBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("PhpCliBaseTimeUsedInMilliseconds", 0);

        public static int PhpCliBaseMemoryUsedInBytes =>
            GetSettingOrDefault("PhpCliBaseMemoryUsedInBytes", 0);

        public static int RubyBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("RubyBaseTimeUsedInMilliseconds", 0);

        public static int RubyBaseMemoryUsedInBytes =>
            GetSettingOrDefault("RubyBaseMemoryUsedInBytes", 0);

        public static int PythonBaseTimeUsedInMilliseconds =>
            GetSettingOrDefault("PythonBaseTimeUsedInMilliseconds", 0);

        public static int PythonBaseMemoryUsedInBytes =>
            GetSettingOrDefault("PythonBaseMemoryUsedInBytes", 0);

        public static int SolidityBaseTimeUsedInMilliseconds => GetSettingOrDefault("SolidityBaseTimeUsedInMilliseconds", 0);

        public static int SolidityBaseMemoryUsedInBytes => GetSettingOrDefault("SolidityBaseMemoryUsedInBytes", 0);

        // Compiler exit time out multipliers
        public static int CPlusPlusCompilerProcessExitTimeOutMultiplier => GetSettingOrDefault("CPlusPlusCompilerProcessExitTimeOutMultiplier", 1);

        public static int CPlusPlusZipCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("CPlusPlusZipCompilerProcessExitTimeOutMultiplier", 1);

        public static int CSharpCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("CSharpCompilerProcessExitTimeOutMultiplier", 1);

        public static int CSharpDotNetCoreCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("CSharpDotNetCoreCompilerProcessExitTimeOutMultiplier", 1);

        public static int DotNetCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("DotNetCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("JavaCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaInPlaceCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("JavaInPlaceCompilerProcessExitTimeOutMultiplier", 1);

        public static int JavaZipCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("JavaZipCompilerProcessExitTimeOutMultiplier", 1);

        public static int MsBuildCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("MsBuildCompilerProcessExitTimeOutMultiplier", 1);

        public static int MsBuildLibraryCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("MsBuildLibraryCompilerProcessExitTimeOutMultiplier", 1);

        public static int SolidityCompilerProcessExitTimeOutMultiplier =>
            GetSettingOrDefault("SolidityCompilerProcessExitTimeOutMultiplier", 1);
    }
}