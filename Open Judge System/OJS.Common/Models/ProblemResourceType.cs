namespace OJS.Common.Models
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum ProblemResourceType
    {
        [LocalizedDescription("ProblemDescription", typeof(Resource))]
        ProblemDescription = 1,

        [LocalizedDescription("AuthorsSolution", typeof(Resource))]
        AuthorsSolution = 2,

        [LocalizedDescription("Video", typeof(Resource))]
        Video = 3,
    }
}
