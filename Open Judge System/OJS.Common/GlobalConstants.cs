namespace OJS.Common
{
    using System.Diagnostics.PerformanceData;

    public static class GlobalConstants
    {
        #region TempData dictionary keys

        public const string InfoMessage = "InfoMessage";
        public const string DangerMessage = "DangerMessage";

        #endregion

        #region ModelState dictionary keys

        public const string DateTimeError = "DateTimeError";

        #endregion

        #region Action names

        public const string Index = "Index";

        #endregion

        #region Partial views

        public const string QuickContestsGrid = "_QuickContestsGrid";

        #endregion

        #region Other

        public const string ExcelMimeTyle = "application/vnd.ms-excel";
        public const string JsonMimeType = "application/json";
        public const string ZipMimeType = "application/zip";
        public const string BinaryFileMimeType = "application/octet-stream";

        public const string AdministratorRoleName = "Administrator";

        public const string EmailRegEx = "^[A-Za-z0-9]+[\\._A-Za-z0-9-]+@([A-Za-z0-9]+[-\\.]?[A-Za-z0-9]+)+(\\.[A-Za-z0-9]+[-\\.]?[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";

        public const int FileExtentionMaxLength = 4;

        public const int IpAdressMaxLength = 45;

        public const int MinimumSearchTermLength = 3;

        public const int FeedbackContentMinLength = 10;

        #endregion

        #region Account

        public const int UserNameMaxLength = 15;
        public const int UserNameMinLength = 5;
        public const string UserNameRegEx = @"^[a-zA-Z]([/._]?[a-zA-Z0-9]+)+$";

        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 100;

        public const int EmailMaxLength = 80;

        public const int FirstNameMaxLength = 30;
        public const int LastNameMaxLength = 30;
        public const int CityNameMaxLength = 30;
        public const int EducationalInstitutionMaxLength = 50;
        public const int FacultyNumberMaxLength = 30;
        public const int CompanyNameMaxLength = 30;
        public const int JobTitleMaxLenth = 30;

        #endregion

        #region News

        public const int NewsTitleMaxLength = 200;
        public const int NewsTitleMinLength = 1;

        public const int NewsAuthorNameMaxLength = 25;
        public const int NewsAuthorNameMinLength = 2;

        public const int NewsSourceMaxLength = 50;
        public const int NewsSourceMinLength = 6;

        public const int NewsContentMinLength = 10;

        #endregion

        #region Contests

        public const int ContestNameMaxLength = 100;
        public const int ContestNameMinLength = 4;

        public const int ContestPasswordMaxLength = 20;

        public const int ContestCategoryNameMaxLength = 100;
        public const int ContestCategoryNameMinLength = 6;

        public const int ContestQuestionMaxLength = 100;
        public const int ContestQuestionMinLength = 1;

        public const int ContestQuestionAnswerMaxLength = 100;
        public const int ContestQuestionAnswerMinLength = 1;

        #endregion

        #region Administration

        public const int CheckerNameMaxLength = 100;
        public const int CheckerNameMinLength = 1;

        public const int ProblemNameMaxLength = 50;

        public const int ProblemDefaultMaximumPoints = 100;
        public const int ProblemDefaultTimeLimit = 1000;
        public const int ProblemDefaultMemoryLimit = 32 * 1024 * 1024;

        public const int ProblemResourceNameMaxLength = 50;
        public const int ProblemResourceNameMinLength = 3;

        public const int SubmissionTypeNameMaxLength = 100;
        public const int SubmissionTypeNameMinLength = 1;

        public const int TestInputMinLength = 1;
        public const int TestOutputMinLength = 1;

        #endregion
    }
}
