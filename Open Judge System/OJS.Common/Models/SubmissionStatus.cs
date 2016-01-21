namespace OJS.Common.Models
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum SubmissionStatus
    {
        [LocalizedDescription("ProcessingSubmissionStatus", typeof(Resource))]
        Processing = 1,

        [LocalizedDescription("ProcessedSubmissionStatus", typeof(Resource))]
        Processed = 2,

        [LocalizedDescription("PendingSubmissionStatus", typeof(Resource))]
        Pending = 3,

        [LocalizedDescription("InvalidSubmissionStatus", typeof(Resource))]
        Invalid = 4
    }
}
