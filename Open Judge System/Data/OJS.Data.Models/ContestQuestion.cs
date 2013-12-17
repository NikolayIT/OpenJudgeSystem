namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Data.Contracts;

    /// <summary>
    /// Represents question that is asked for every participant (real or practice) in the specified contest.
    /// </summary>
    public class ContestQuestion : DeletableEntity
    {
        private ICollection<ContestQuestionAnswer> answers;
        private ICollection<ParticipantAnswer> participantAnswers;

        public ContestQuestion()
        {
            this.answers = new HashSet<ContestQuestionAnswer>();
            this.participantAnswers = new HashSet<ParticipantAnswer>();
        }

        public int Id { get; set; }

        [ForeignKey("Contest")]
        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; }
        
        public string Text { get; set; }

        [DefaultValue(true)]
        public bool AskOfficialParticipants { get; set; }

        [DefaultValue(true)]
        public bool AskPracticeParticipants { get; set; }

        public ContestQuestionType Type { get; set; }

        public virtual ICollection<ContestQuestionAnswer> Answers
        {
            get { return this.answers; }
            set { this.answers = value; }
        }

        public virtual ICollection<ParticipantAnswer> ParticipantAnswers
        {
            get { return this.participantAnswers; }
            set { this.participantAnswers = value; }
        }

        public string RegularExpressionValidation { get; set; }
    }
}
