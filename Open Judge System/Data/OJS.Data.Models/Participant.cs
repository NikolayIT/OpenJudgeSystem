namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class Participant : AuditInfo
    {
        private ICollection<Submission> submissions;
        private ICollection<ParticipantAnswer> answers;

        public Participant()
        {
            this.submissions = new HashSet<Submission>();
            this.answers = new HashSet<ParticipantAnswer>();
        }

        public Participant(int contestId, string userId, bool isOfficial)
            : this()
        {
            this.ContestId = contestId;
            this.UserId = userId;
            this.IsOfficial = isOfficial;
        }

        public int Id { get; set; }

        public int OldId { get; set; }

        public int ContestId { get; set; }

        public string UserId { get; set; }

        public bool IsOfficial { get; set; }

        [Required]
        public virtual Contest Contest { get; set; }

        public virtual UserProfile User { get; set; }

        public virtual ICollection<Submission> Submissions
        {
            get { return this.submissions; }
            set { this.submissions = value; }
        }

        public virtual ICollection<ParticipantAnswer> Answers
        {
            get { return this.answers; }
            set { this.answers = value; }
        }
    }
}
