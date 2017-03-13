namespace SubmissionScoresMigrator
{
    public class SubmissionScoreModel
    {
        public int SubmissionId { get; set; }

        public int? ProblemId { get; set; }

        public int? ParticipantId { get; set; }

        public string ParticipantName { get; set; }

        public int Points { get; set; }

        public bool IsOfficial { get; set; }
    }
}
