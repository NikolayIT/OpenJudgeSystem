namespace OJS.Web.Common
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum TestType
    {
        [LocalizedDescription("StandardTestDescription", typeof(Resource))]
        Standard = 1,

        [LocalizedDescription("OpenTestDescription", typeof(Resource))]
        Open = 2,

        [LocalizedDescription("TrialTestDescription", typeof(Resource))]
        Trial = 3
    }
}
