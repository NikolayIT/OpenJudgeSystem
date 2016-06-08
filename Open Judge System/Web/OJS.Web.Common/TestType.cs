namespace OJS.Web.Common
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum TestType
    {
        [LocalizedDescription("StandartTestDescription", typeof(Resource))]
        Standart = 1,

        [LocalizedDescription("OpenTestDescription", typeof(Resource))]
        Open = 2,

        [LocalizedDescription("TrialTestDescription", typeof(Resource))]
        Trial = 3
    }
}
