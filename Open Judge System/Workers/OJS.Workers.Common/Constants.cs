namespace OJS.Workers.Common
{
    using System;
    using System.IO;

    using OJS.Workers.Common.Extensions;
    using OJS.Workers.Common.Models;

    public static class Constants
    {
        public const string LocalWorkerServiceName = "OJS Local Worker Service";
        public const string LocalWorkerMonitoringServiceName = "OJS Local Worker Monitoring Service";

        public const string LocalWorkerServiceLogName = "LocalWorkerService";
        public const string LocalWorkerMonitoringServiceLogName = "LocalWorkerMonitoringService";

        public const int DefaultJobLoopWaitTimeInMilliseconds = 1000;
        public const int DefaultTimeBeforeAbortingThreadsInMilliseconds = 10000;

        public const int DefaultTimeLimitInMilliseconds = 100;
        public const int DefaultMemoryLimitInBytes = 16 * 1024 * 1024;

        // File extensions
        public const string ClassLibraryFileExtension = ".dll";
        public const string ExecutableFileExtension = ".exe";
        public const string JavaScriptFileExtension = ".js";
        public const string ZipFileExtension = ".zip";
        public const string JsonFileExtension = ".json";
        public const string SolidityFileExtension = ".sol";
        public const string ByteCodeFileExtension = ".bin";
        public const string AbiFileExtension = ".abi";

        // Folder names
        public const string ExecutionStrategiesFolderName = "ExecutionStrategies";

        // Other
        public const int DefaultProcessExitTimeOutMilliseconds = 5000; // ms

        // Runtime constants
        public static readonly string JavaSourceFileExtension = $".{CompilerType.Java.GetFileExtension()}";
        public static readonly string CSharpFileExtension = $".{CompilerType.CSharp.GetFileExtension()}";
        public static readonly string ClassDelimiter = $"~~!!!==#==!!!~~{Environment.NewLine}";

        // Temp Directory folder paths
        public static readonly string ExecutionStrategiesWorkingDirectoryPath =
            Path.Combine(
                Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine),
                ExecutionStrategiesFolderName);
    }
}