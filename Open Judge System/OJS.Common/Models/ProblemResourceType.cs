namespace OJS.Common.Models
{
    using OJS.Common.Attributes;

    public enum ProblemResourceType
    {
        [LocalizedDescription("Условие")]
        ProblemDescription = 1,

        [LocalizedDescription("Авторско решение")]
        AuthorsSolution = 2,

        [LocalizedDescription("Видео")]
        Video = 3,
    }
}
