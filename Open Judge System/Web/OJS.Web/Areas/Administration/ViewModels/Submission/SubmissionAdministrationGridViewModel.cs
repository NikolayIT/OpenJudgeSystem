namespace OJS.Web.Areas.Administration.ViewModels.Submission
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using Resource = Resources.Areas.Administration.Submissions.ViewModels.SubmissionAdministration;

    public class SubmissionAdministrationGridViewModel : AdministrationViewModel<Submission>
    {
        [ExcludeFromExcel]
        public static Expression<Func<Submission, SubmissionAdministrationGridViewModel>> ViewModel
        {
            get
            {
                return sub => new SubmissionAdministrationGridViewModel
                {
                    Id = sub.Id,
                    ParticipantName = sub.Participant.User.UserName,
                    ProblemId = sub.ProblemId,
                    ProblemName = sub.Problem.Name,
                    ContestName = sub.Problem.ProblemGroup.Contest.Name,
                    ContestId = sub.Problem.ProblemGroup.ContestId,
                    SubmissionTypeName = sub.SubmissionType.Name,
                    Points = sub.Points,
                    Status = sub.Processed
                        ? SubmissionStatus.Processed
                        : SubmissionStatus.Pending,
                    CreatedOn = sub.CreatedOn,
                    ModifiedOn = sub.ModifiedOn
                };
            }
        }

        [Display(Name = "№")]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        public int? ProblemId { get; set; }

        [Display(Name = "Problem", ResourceType = typeof(Resource))]
        public string ProblemName { get; set; }

        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        public string ContestName { get; set; }

        public int ContestId { get; set; }

        [Display(Name = "Participant", ResourceType = typeof(Resource))]
        public string ParticipantName { get; set; }

        [Display(Name = "Type", ResourceType = typeof(Resource))]
        public string SubmissionTypeName { get; set; }

        [Display(Name = "Points", ResourceType = typeof(Resource))]
        public int? Points { get; set; }

        [Display(Name = "Status", ResourceType = typeof(Resource))]
        public SubmissionStatus Status { get; set; }

        public string ContestUrlName => this.ContestName?.ToUrl();
    }
}