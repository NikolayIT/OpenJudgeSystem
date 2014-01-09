namespace OJS.Web.Areas.Administration.ViewModels.Participant
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using OJS.Common.DataAnnotations;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel;
    using System.Web.Mvc;

    public class ParticipantAdministrationViewModel : AdministrationViewModel<Participant>
    {
        [ExcludeFromExcel]
        public static Expression<Func<Participant, ParticipantAdministrationViewModel>> ViewModel
        {
            get
            {
                return p => new ParticipantAdministrationViewModel
                {
                    Id = p.Id,
                    ContestId = p.ContestId,
                    ContestName = p.Contest.Name,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    IsOfficial = p.IsOfficial,
                    CreatedOn = p.CreatedOn,
                    ModifiedOn = p.ModifiedOn,
                };
            }
        }

        [DatabaseProperty]
        [Display(Name = "№")]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Състезание")]
        [Required(ErrorMessage = "Състезанието е задължително!")]
        [UIHint("ContestsComboBox")]
        public int ContestId { get; set; }

        [Display(Name = "Състезание")]
        [HiddenInput(DisplayValue = false)]
        public string ContestName { get; set; }

        [DatabaseProperty]
        [Display(Name = "Потребител")]
        [Required(ErrorMessage = "Потребителят е задължителен!")]
        [UIHint("UsersComboBox")]
        public string UserId { get; set; }

        [Display(Name = "Потребител")]
        [HiddenInput(DisplayValue = false)]
        public string UserName { get; set; }

        [DatabaseProperty]
        [Display(Name = "Официално участие")]
        public bool IsOfficial { get; set; }
    }
}