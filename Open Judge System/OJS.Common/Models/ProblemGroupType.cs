namespace OJS.Common.Models
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum ProblemGroupType
    {
        [LocalizedDescription("None", typeof(Resource))]
        None = 0,

        [LocalizedDescription("Excluded_from_export_problem_group", typeof(Resource))]
        ExcludedFromExport = 1
    }
}