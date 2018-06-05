namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ParticipantScore
    {
        [Key]
        public int Id { get; set; }

        public int ProblemId { get; set; }

        public virtual Problem Problem { get; set; }

        public int ParticipantId { get; set; }

        public virtual Participant Participant { get; set; }

        public int? SubmissionId { get; set; }

        public virtual Submission Submission { get; set; }

        public string ParticipantName { get; set; }

        public int Points { get; set; }

        [Index]
        public bool IsOfficial { get; set; }
    }
}
