namespace OJS.Web.Areas.Administration.ViewModels.Submission
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Common.Extensions;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class SubmissionAdministrationViewModel : AdministrationViewModel<Submission>
    {
        [ExcludeFromExcel]
        public static Expression<Func<Submission, SubmissionAdministrationViewModel>> ViewModel
        {
            get
            {
                return sub => new SubmissionAdministrationViewModel
                {
                    Id = sub.Id,
                    ProblemId = sub.ProblemId,
                    ParticipantId = sub.ParticipantId,
                    SubmissionTypeId = sub.SubmissionTypeId,
                    Content = sub.Content,
                    CreatedOn = sub.CreatedOn,
                    ModifiedOn = sub.ModifiedOn,
                };
            }
        }

        [DatabaseProperty]
        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Задача")]
        [Required(ErrorMessage = "Задачата е задължителна!")]
        [UIHint("ProblemComboBox")]
        public int? ProblemId { get; set; }

        [DatabaseProperty]
        [Display(Name = "Потребител")]
        [Required(ErrorMessage = "Потребителя е задължителен!")]
        [UIHint("ParticipantDropDownList")]
        public int? ParticipantId { get; set; }

        [DatabaseProperty]
        [Display(Name = "Тип")]
        [Required(ErrorMessage = "Типа е задължителен!")]
        [UIHint("SubmissionTypesDropDownList")]
        public int? SubmissionTypeId { get; set; }

        [DatabaseProperty]
        [ScaffoldColumn(false)]
        public byte[] Content { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Съдържанието е задължително!")]
        [UIHint("MultiLineText")]
        public string ContentAsString
        {
            get
            {
                return this.Content.Decompress();
            }

            set
            {
                this.Content = value.Compress();
            }
        }
    }
}