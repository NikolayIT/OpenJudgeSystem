namespace OJS.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq.Expressions;

    using OJS.Common;

    [Table("Submissions")]
    public class ArchivedSubmission
    {
        public static Expression<Func<Submission, ArchivedSubmission>> FromSubmission =>
            submission => new ArchivedSubmission
            {
                Id = submission.Id,
                ParticipantId = submission.ParticipantId,
                ProblemId = submission.ProblemId,
                SubmissionTypeId = submission.SubmissionTypeId,
                Content = submission.Content,
                FileExtension = submission.FileExtension,
                SolutionSkeleton = submission.SolutionSkeleton,
                IpAddress = submission.IpAddress,
                IsCompiledSuccessfully = submission.IsCompiledSuccessfully,
                CompilerComment = submission.CompilerComment,
                IsPublic = submission.IsPublic,
                TestRunsCache = submission.TestRunsCache,
                Processed = submission.Processed,
                ProcessingComment = submission.ProcessingComment,
                Points = submission.Points,
                IsDeleted = submission.IsDeleted,
                DeletedOn = submission.DeletedOn,
                CreatedOn = submission.CreatedOn,
                ModifiedOn = submission.ModifiedOn
            };

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

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        [Index]
        public bool IsHardDeletedFromMainDatabase { get; set; }
    }
}