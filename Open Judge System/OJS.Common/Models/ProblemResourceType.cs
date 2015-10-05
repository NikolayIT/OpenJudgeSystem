namespace OJS.Common.Models
{
    using System.ComponentModel;

    public enum ProblemResourceType
    {
        [Description("Условие")]
        ProblemDescription = 1,

        [Description("Авторско решение")]
        AuthorsSolution = 2,

        [Description("Линк")]
        Link = 3,
    }
}
