namespace OJS.Common.Models
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum SubmissionStatus
    {
        [LocalizedDescription("ProcessedSubmissionStatus", typeof(Resource))]
        Processed = 1,

        [LocalizedDescription("PendingSubmissionStatus", typeof(Resource))]
        Pending = 2
    }
}