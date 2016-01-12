namespace OJS.Common.Models
{
    using System.ComponentModel;

    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum ProblemResourceType
    {
        [LocalizedDescription("ProblemDescription", typeof(Resource))]
        ProblemDescription = 1,

        [LocalizedDescription("AuthorsSolution", typeof(Resource))]
        AuthorsSolution = 2,

        [Description("Линк")]
        Link = 3,
    }
}
