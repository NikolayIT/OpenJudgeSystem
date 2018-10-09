namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Services.Cache.Models;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;

    using Resource = Resources.Areas.Contests.ViewModels.ContestsViewModels;

    public class ContestViewModel
    {
        private string contestName;

        public static Expression<Func<Contest, ContestViewModel>> FromContest =>
            contest => new ContestViewModel
            {
                Id = contest.Id,
                Name = contest.Name,
                CategoryId = contest.CategoryId,
                CategoryName = contest.Category.Name,
                StartTime = contest.StartTime,
                EndTime = contest.EndTime,
                PracticeStartTime = contest.PracticeStartTime,
                PracticeEndTime = contest.PracticeEndTime,
                IsDeleted = contest.IsDeleted,
                IsVisible = contest.IsVisible,
                IsOnline = contest.Type == ContestType.OnlinePracticalExam,
                ContestPassword = contest.ContestPassword,
                PracticePassword = contest.PracticePassword,
                HasContestQuestions = contest.Questions.Any(x => x.AskOfficialParticipants),
                HasPracticeQuestions = contest.Questions.Any(x => x.AskPracticeParticipants),
                ContestType = contest.Type,
                OfficialParticipants = contest.Participants.Count(x => x.IsOfficial),
                PracticeParticipants = contest.Participants.Count(x => !x.IsOfficial),
                ProblemsCount = contest.ProblemGroups.SelectMany(pg => pg.Problems).Count(p => !p.IsDeleted),
                Problems = contest.ProblemGroups
                    .SelectMany(pg => pg.Problems)
                    .AsQueryable()
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.ProblemGroup.OrderBy)
                    .ThenBy(p => p.OrderBy)
                    .ThenBy(p => p.Name)
                    .Select(ContestProblemViewModel.FromProblem),
                LimitBetweenSubmissions = contest.LimitBetweenSubmissions,
                Description = contest.Description,
                AllowedSubmissionTypes = contest.ProblemGroups
                    .SelectMany(pg => pg.Problems)
                    .AsQueryable()
                    .SelectMany(p => p.SubmissionTypes)
                    .GroupBy(st => st.Id)
                    .Select(g => g.FirstOrDefault())
                    .Select(SubmissionTypeViewModel.FromSubmissionType)
            };

        public int Id { get; set; }

        [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
        public string Name { get; set; }

        public int Type { get; set; }

        public int? CategoryId { get; set; }

        public string CategoryName
        {
            get => this.contestName.ToUrl();

            set => this.contestName = value;
        }

        public string Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime? PracticeStartTime { get; set; }

        public DateTime? PracticeEndTime { get; set; }

        public int LimitBetweenSubmissions { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsVisible { get; set; }

        public bool IsOnline { get; set; }

        public string ContestPassword { private get; set; }

        public string PracticePassword { private get; set; }

        public bool HasContestQuestions { get; set; }

        public bool HasPracticeQuestions { get; set; }

        public int OfficialParticipants { get; set; }

        public int PracticeParticipants { get; set; }

        public int ProblemsCount { get; set; }

        public ContestType ContestType { get; set; }

        public IEnumerable<SubmissionTypeViewModel> AllowedSubmissionTypes { get; set; }

        public IEnumerable<ContestProblemViewModel> Problems { get; set; }

        public IEnumerable<ContestCategoryListViewModel> ParentCategories { get; set; } =
            Enumerable.Empty<ContestCategoryListViewModel>();

        public bool CanBeCompeted
        {
            get
            {
                if (!this.IsVisible)
                {
                    return false;
                }

                if (this.IsDeleted)
                {
                    return false;
                }

                if (!this.StartTime.HasValue)
                {
                    // Cannot be competed
                    return false;
                }

                if (!this.EndTime.HasValue)
                {
                    // Compete forever
                    return this.StartTime <= DateTime.Now;
                }

                return this.StartTime <= DateTime.Now && DateTime.Now <= this.EndTime;
            }
        }

        public bool CanBePracticed
        {
            get
            {
                if (!this.IsVisible)
                {
                    return false;
                }

                if (this.IsDeleted)
                {
                    return false;
                }

                if (!this.PracticeStartTime.HasValue)
                {
                    // Cannot be practiced
                    return false;
                }

                if (!this.PracticeEndTime.HasValue)
                {
                    // Practice forever
                    return this.PracticeStartTime <= DateTime.Now;
                }

                return this.PracticeStartTime <= DateTime.Now && DateTime.Now <= this.PracticeEndTime;
            }
        }

        public bool ResultsArePubliclyVisible
        {
            get
            {
                if (!this.IsVisible)
                {
                    return false;
                }

                if (this.IsDeleted)
                {
                    return false;
                }

                if (!this.StartTime.HasValue)
                {
                    // Cannot be competed
                    return false;
                }

                return this.EndTime.HasValue && this.EndTime.Value <= DateTime.Now;
            }
        }

        public bool HasContestPassword => this.ContestPassword != null;

        public bool HasPracticePassword => this.PracticePassword != null;

        public double? RemainingTimeInMilliseconds
        {
            get
            {
                if (this.EndTime.HasValue)
                {
                    return (this.EndTime.Value - DateTime.Now).TotalMilliseconds;
                }

                return null;
            }
        }

        public bool UserIsAdminOrLecturerInContest { get; set; }

        public bool UserCanCompete { get; set; }

        public bool UserIsParticipant { get; set; }

        public bool IsActive { get; set; }
    }
}