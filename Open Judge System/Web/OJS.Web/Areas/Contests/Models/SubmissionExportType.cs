namespace OJS.Web.Areas.Contests.Models
{
    using System.ComponentModel;

    public enum SubmissionExportType
    {
        [Description("Всички решения")]
        AllSubmissions = 1,

        [Description("Най-добри решения")]
        BestSubmissions = 2,

        [Description("Последни решения")]
        LastSubmissions = 3,
    }
}