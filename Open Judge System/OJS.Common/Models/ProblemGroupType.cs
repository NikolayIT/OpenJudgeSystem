namespace OJS.Common.Models
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum ProblemGroupType
    {
        [LocalizedDescription("Excluded_from_homework_problem_group", typeof(Resource))]
        ExcludedFromHomework = 1
    }
}