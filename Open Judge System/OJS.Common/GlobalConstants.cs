namespace OJS.Common
{
    public static class GlobalConstants
    {
        #region User Profile Constants

        public const int UsernameMinLength = 5;
        public const int UsernameMaxLength = 32;
        public const string UsernameRegEx = @"^[a-zA-Z]([/._]?[a-zA-Z0-9]+)+$";

        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 1000;

        public const int EmailMaxLength = 80;
        public const int EmailMinLength = 6;
        public const string EmailRegEx = "^[A-Za-z0-9]+[\\._A-Za-z0-9-]+@([A-Za-z0-9]+[-\\.]?[A-Za-z0-9]+)+(\\.[A-Za-z0-9]+[-\\.]?[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";

        public const int NameMinLength = 2;
        public const int NameMaxLength = 30;
        
        public const int CityMinLength = 3;
        public const int CityMaxLength = 50;
        public const string CityRegEx = @"^[a-zA-Zа-яА-Я]+(?:[\s-][a-zA-Zа-яА-Я]+)*$";

        public const int CompanyMinLength = 2;
        public const int CompanyMaxLength = 100;
        public const string CompanyRegEx = @"^([a-zA-Zа-яА-Я0-9]|[- @\.#&!""])*$";

        public const int JobTitleMinLength = 2;
        public const int JobTitleMaxLength = 100;
        public const string JobTitleRegEx = @"^([a-zA-Zа-яА-Я0-9]|[- @\.#&!])*$";

        #endregion

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
        public const string LecturerRoleName = "Lecturer";

        public const int MinimumSearchTermLength = 3;

        public const string AuthCookieName = ".AspNet.SoftUniJudgeCookie";

        #endregion
    }
}
