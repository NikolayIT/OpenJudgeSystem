namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    using Resource = Resources.Areas.Contests.ViewModels.ProblemsViewModels;

    public class ProblemResultViewModel
    {
        public static Expression<Func<Submission, ProblemResultViewModel>> FromSubmission
        {
            get
            {
                return submission =>
                    new ProblemResultViewModel
                    {
                        ProblemId = submission.Problem.Id,
                        SubmissionId = submission.Id,
                        ParticipantName = submission.Participant.User.UserName,
                        MaximumPoints = submission.Problem.MaximumPoints,
                        Result = submission.Points
                    };
            }
        }

        public int? SubmissionId { get; set; }

        [Display(Name = "Participant", ResourceType = typeof(Resource))]
        public string ParticipantName { get; set; }

        [Display(Name = "Result", ResourceType = typeof(Resource))]
        public int Result { get; set; }

        public int MaximumPoints { get; set; }

        public int ProblemId { get; set; }
    }
}