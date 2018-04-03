namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data.Contracts;

    /// <summary>
    /// Represents question that is asked for every participant (real or practice) in the specified contest.
    /// </summary>
    public class ContestQuestion : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; }

        [MaxLength(GlobalConstants.ContestQuestionMaxLength)]
        [MinLength(GlobalConstants.ContestQuestionMinLength)]
        public string Text { get; set; }

        [DefaultValue(true)]
        public bool AskOfficialParticipants { get; set; }

        [DefaultValue(true)]
        public bool AskPracticeParticipants { get; set; }

        public ContestQuestionType Type { get; set; }

        public virtual ICollection<ContestQuestionAnswer> Answers { get; set; } = new HashSet<ContestQuestionAnswer>();

        public virtual ICollection<ParticipantAnswer> ParticipantAnswers { get; set; } = new HashSet<ParticipantAnswer>();

        public string RegularExpressionValidation { get; set; }
    }
}
