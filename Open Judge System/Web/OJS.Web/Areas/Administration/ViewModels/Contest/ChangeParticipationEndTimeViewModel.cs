namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.SqlServer;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Contests.Views.ChangeTime;

    public class ChangeParticipationEndTimeViewModel
    {
        private static readonly int DefaultBufferTimeInMinutes = 30;

        public ChangeParticipationEndTimeViewModel()
        {
        }

        public ChangeParticipationEndTimeViewModel(
            ChangeParticipationEndTimeByTimeIntervalViewModel changeByIntervalModel)
        {
            this.ContesId = changeByIntervalModel.ContesId;
            this.ContestName = changeByIntervalModel.ContestName;
            this.TimeInMinutes = changeByIntervalModel.TimeInMinutes;
            this.ChangeByInterval = changeByIntervalModel;
            this.ChangeByUser = new ChangeParticipationEndTimeByUserViewModel
            {
                ContesId = this.ContesId,
                ContestName = this.ContestName
            };
        }

        public ChangeParticipationEndTimeViewModel(ChangeParticipationEndTimeByUserViewModel changebyUserModel)
        {
            this.ContesId = changebyUserModel.ContesId;
            this.ContestName = changebyUserModel.ContestName;
            this.TimeInMinutes = changebyUserModel.TimeInMinutes;
            this.ChangeByUser = changebyUserModel;
            this.ChangeByInterval = new ChangeParticipationEndTimeByTimeIntervalViewModel
            {
                ContesId = this.ContesId,
                ContestName = this.ContestName
            };
        }

        public static Expression<Func<Contest, ChangeParticipationEndTimeViewModel>> FromContest =>
            contest => new ChangeParticipationEndTimeViewModel
            {
                ContesId = contest.Id,
                ContestName = contest.Name,
                ChangeByInterval = new ChangeParticipationEndTimeByTimeIntervalViewModel
                {
                    ContesId = contest.Id,
                    ContestName = contest.Name,
                    ParticipantsCreatedBeforeDateTime = DateTime.Now,
                    ParticipantsCreatedAfterDateTime = SqlFunctions.DateAdd(
                        "minute",
                        ((contest.Duration.Value.Hours * 60) + contest.Duration.Value.Minutes + DefaultBufferTimeInMinutes) * -1,
                        DateTime.Now)
                },
                ChangeByUser = new ChangeParticipationEndTimeByUserViewModel
                {
                    ContesId = contest.Id,
                    ContestName = contest.Name
                }
            };

        [HiddenInput(DisplayValue = false)]
        public int ContesId { get; set; }

        [Display(Name = nameof(Resource.Contest), ResourceType = typeof(Resource))]
        public string ContestName { get; set; }

        [Display(Name = nameof(Resource.Time_in_minutes_information), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Time_required_error),
            ErrorMessageResourceType = typeof(Resource))]
        public int TimeInMinutes { get; set; }

        public ChangeParticipationEndTimeByTimeIntervalViewModel ChangeByInterval { get; set; }

        public ChangeParticipationEndTimeByUserViewModel ChangeByUser { get; set; }
    }
}