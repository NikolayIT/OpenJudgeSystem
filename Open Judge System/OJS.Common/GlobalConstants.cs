namespace OJS.Common
{
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

        public const string AdministratorRoleName = "Administrator";

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
    }
}
