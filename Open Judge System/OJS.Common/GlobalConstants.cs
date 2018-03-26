namespace OJS.Common
{
    using System;
    using System.IO;

    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public static class GlobalConstants
    {
        // User Profile Constants
        public const int UsernameMinLength = 5;
        public const int UsernameMaxLength = 32;
        public const string UsernameRegEx = @"^[a-zA-Z]([/._]?[a-zA-Z0-9]+)+$";

        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 1000;

        public const int EmailMinLength = 6;
        public const int EmailMaxLength = 80;
        public const string EmailRegEx = "^[A-Za-z0-9]+[\\._A-Za-z0-9-]+@([A-Za-z0-9]+[-\\.]?[A-Za-z0-9]+)+(\\.[A-Za-z0-9]+[-\\.]?[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";

        public const int NameMinLength = 2;
        public const int NameMaxLength = 30;

        public const int CityMinLength = 2;
        public const int CityMaxLength = 200;
        public const string CityRegEx = @"^[a-zA-Zа-яА-Я]+(?:[\s-][a-zA-Zа-яА-Я]+)*$";

        public const int CompanyMinLength = 2;
        public const int CompanyMaxLength = 200;
        public const string CompanyRegEx = @"^([a-zA-Zа-яА-Я0-9]|[- @\.#&!""])*$";
        public const int JobTitleMinLength = 2;
        public const int JobTitleMaxLength = 100;
        public const string JobTitleRegEx = @"^([a-zA-Zа-яА-Я0-9]|[- @\.#&!])*$";
        public const int FacultyNumberMaxLength = 30;

        // TempData dictionary keys
        public const string InfoMessage = "InfoMessage";
        public const string DangerMessage = "DangerMessage";

        // ModelState dictionary keys
        public const string DateTimeError = "DateTimeError";

        // Action names
        public const string Index = "Index";

        // Partial views
        public const string QuickContestsGrid = "_QuickContestsGrid";

        // Other
        public const string ExcelMimeType = "application/vnd.ms-excel";
        public const string JsonMimeType = "application/json";
        public const string ZipMimeType = "application/zip";
        public const string BinaryFileMimeType = "application/octet-stream";

        public const string AdministratorRoleName = "Administrator";

        public const string AdministrationAreaName = "Administration";

        public const int FileExtentionMaxLength = 4;

        public const int IpAdressMaxLength = 45;

        public const int MinimumSearchTermLength = 3;

        public const int FeedbackContentMinLength = 10;

        public const string LecturerRoleName = "Lecturer";
        public const string DefaultPublicIp = "217.174.159.226";
        public const string AuthCookieName = ".AspNet.SoftUniJudgeCookie";

        public const int DefaultProcessExitTimeOutMilliseconds = 5000; // ms

        public const int DefaultProblemGroupsCountForOnlineContest = 2;

        // File extensions
        public const string JavaCompiledFileExtension = ".class";
        public const string ZipFileExtension = ".zip";
        public const string ExecutableFileExtension = ".exe";
        public const string ClassLibraryFileExtension = ".dll";

        // Folder names
        public const string ExecutionStrategiesFolderName = "ExecutionStrategies";

        // Contests
        public const int ContestNameMaxLength = 100;
        public const int ContestNameMinLength = 4;

        public const int ContestPasswordMaxLength = 20;

        public const int ContestCategoryNameMaxLength = 100;
        public const int ContestCategoryNameMinLength = 6;

        public const int ContestQuestionMaxLength = 100;
        public const int ContestQuestionMinLength = 1;

        public const int ContestQuestionAnswerMaxLength = 100;
        public const int ContestQuestionAnswerMinLength = 1;

        // ExamGroup Constants
        public const int ExamGroupNameMinLength = 2;
        public const int ExamGroupNameMaxLength = 600;

        // Administration
        public const int CheckerNameMaxLength = 100;
        public const int CheckerNameMinLength = 1;

        public const int ProblemNameMaxLength = 50;

        public const string ProblemDefaultName = "Име";
        public const int ProblemDefaultOrderBy = 0;
        public const int ProblemDefaultMaximumPoints = 100;
        public const int ProblemDefaultTimeLimit = 100;
        public const int ProblemDefaultMemoryLimit = 16 * 1024 * 1024;
        public const int ProblemDefaultSourceLimit = 16384;
        public const bool ProblemDefaultShowResults = true;
        public const bool ProblemDefaultShowDetailedFeedback = false;

        public const int ProblemResourceNameMaxLength = 50;
        public const int ProblemResourceNameMinLength = 3;

        public const int SubmissionTypeNameMaxLength = 100;
        public const int SubmissionTypeNameMinLength = 1;

        public const int TestInputMinLength = 1;
        public const int TestOutputMinLength = 1;

        // Date and time formats
        public const string DefaultDateTimeFormatString = "{0:dd/MM/yyyy HH:mm}";

        // Runtime constants
        public static readonly string JavaSourceFileExtension = $".{CompilerType.Java.GetFileExtension()}";
        public static readonly string CSharpFileExtension = $".{CompilerType.CSharp.GetFileExtension()}";
        public static readonly string ClassDelimiter = $"~~!!!==#==!!!~~{Environment.NewLine}";

        // Temp Directory folder paths
        public static readonly string ExecutionStrategiesWorkingDirectoryPath =
            Path.Combine(Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine),
                ExecutionStrategiesFolderName);
    }
}