namespace OJS.Common.Models
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum ContestType
    {
        [LocalizedDescription("Onsite_practical_exam", typeof(Resource))]
        OnsitePractialExam = 1,

        [LocalizedDescription("Online_practical_exam", typeof(Resource))]
        OnlinePractialExam = 2,

        [LocalizedDescription("Lab", typeof(Resource))]
        Lab = 3
    }
}