namespace OJS.Web.Areas.Administration.ViewModels.Participant
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using Resource = Resources.Areas.Administration.Participants.ViewModels.ParticipantViewModels;

    public class ParticipantAdministrationViewModel : AdministrationViewModel<Participant>
    {
        [ExcludeFromExcel]
        public static Expression<Func<Participant, ParticipantAdministrationViewModel>> ViewModel =>
            participant => new ParticipantAdministrationViewModel
            {
                Id = participant.Id,
                ContestId = participant.ContestId,
                ContestName = participant.Contest.Name,
                ParticipationStartTime = participant.ParticipationStartTime,
                ParticipationEndTime = participant.ParticipationEndTime,
                UserId = participant.UserId,
                UserName = participant.User.UserName,
                IsOfficial = participant.IsOfficial,
                CreatedOn = participant.CreatedOn,
                ModifiedOn = participant.ModifiedOn
            };

        [DatabaseProperty]
        [Display(Name = "№")]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Contest_required",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("ContestsComboBox")]
        public int ContestId { get; set; }

        [Display(Name = "Contest_name", ResourceType = typeof(Resource))]
        [HiddenInput(DisplayValue = false)]
        public string ContestName { get; set; }

        [Display(Name = "Participation_start_time", ResourceType = typeof(Resource))]
        [HiddenInput(DisplayValue = false)]
        public DateTime? ParticipationStartTime { get; set; }

        [Display(Name = "Participation_end_time", ResourceType = typeof(Resource))]
        [HiddenInput(DisplayValue = false)]
        public DateTime? ParticipationEndTime { get; set; }

        [DatabaseProperty]
        [Display(Name = "User", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "User_required",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("UsersComboBox")]
        public string UserId { get; set; }

        [Display(Name = "UserName", ResourceType = typeof(Resource))]
        [HiddenInput(DisplayValue = false)]
        public string UserName { get; set; }

        [DatabaseProperty]
        [Display(Name = "Is_official", ResourceType = typeof(Resource))]
        public bool IsOfficial { get; set; }
    }
}