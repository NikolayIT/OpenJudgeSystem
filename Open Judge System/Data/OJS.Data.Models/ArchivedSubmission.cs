namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using OJS.Common;

    [Table("Submissions")]
    public class ArchivedSubmission
    {
        public ArchivedSubmission(Submission submission)
        {
            this.Id = submission.Id;
            this.ParticipantId = submission.ParticipantId;
            this.ProblemId = submission.ProblemId;
            this.SubmissionTypeId = submission.SubmissionTypeId;
            this.Content = submission.Content;
            this.FileExtension = submission.FileExtension;
            this.SolutionSkeleton = submission.SolutionSkeleton;
            this.IpAddress = submission.IpAddress;
            this.IsCompiledSuccessfully = submission.IsCompiledSuccessfully;
            this.CompilerComment = submission.CompilerComment;
            this.IsPublic = submission.IsPublic;
            this.TestRunsCache = submission.TestRunsCache;
            this.Processed = submission.Processed;
            this.ProcessingComment = submission.ProcessingComment;
            this.Points = submission.Points;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParticipantId { get; set; }

        public int? ProblemId { get; set; }

        public int? SubmissionTypeId { get; set; }

        public byte[] Content { get; set; }

        public string FileExtension { get; set; }

        public byte[] SolutionSkeleton { get; set; }

        [StringLength(GlobalConstants.IpAdressMaxLength)]
        [Column(TypeName = "varchar")]
        public string IpAddress { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public string CompilerComment { get; set; }

        public bool? IsPublic { get; set; }

        public string TestRunsCache { get; set; }

        public bool Processed { get; set; }

        public string ProcessingComment { get; set; }

        public int Points { get; set; }
    }
}