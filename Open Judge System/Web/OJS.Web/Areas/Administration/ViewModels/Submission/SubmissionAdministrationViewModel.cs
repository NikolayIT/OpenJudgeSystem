namespace OJS.Web.Areas.Administration.ViewModels.Submission
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.Extensions;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using OJS.Common.DataAnnotations;

    public class SubmissionAdministrationViewModel : AdministrationViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Submission, SubmissionAdministrationViewModel>> ViewModel
        {
            get
            {
                return sub => new SubmissionAdministrationViewModel
                {
                    Id = sub.Id,
                    ParticipantId = sub.ParticipantId.Value,
                    ParticipantName = sub.Participant.User.UserName,
                    ProblemId = sub.ProblemId.Value,
                    ProblemName = sub.Problem.Name,
                    Content = sub.Content,
                    SubmissionTypeId = sub.SubmissionTypeId.Value,
                    SubmissionTypeName = sub.SubmissionType.Name,
                    Points = sub.Points,
                    Processed = sub.Processed,
                    Processing = sub.Processing,
                    CreatedOn = sub.CreatedOn,
                    ModifiedOn = sub.ModifiedOn,
                };
            }
        }

        [ExcludeFromExcel]
        public Submission ToEntity
        {
            get
            {
                return new Submission
                {
                    Id = this.Id ?? default(int),
                    ParticipantId = this.ParticipantId,
                    ProblemId = this.ProblemId,
                    Content = this.Content,
                    SubmissionTypeId = this.SubmissionTypeId,
                    Processed = false,
                    Processing = false,
                    CreatedOn = this.CreatedOn,
                    ModifiedOn = this.ModifiedOn,
                };
            }
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [Display(Name = "Потребител")]
        [Required(ErrorMessage = "Потребителя е задължителен!")]
        [UIHint("DropDownList")]
        [DefaultValue(0)]
        public int ParticipantId { get; set; }

        [Display(Name = "Потребител")]
        [HiddenInput(DisplayValue = false)]
        public string ParticipantName { get; set; }

        [Display(Name = "Задача")]
        [Required(ErrorMessage = "Задачата е задължителна!")]
        [UIHint("DropDownList")]
        [DefaultValue(0)]
        public int ProblemId { get; set; }

        [Display(Name = "Задача")]
        [HiddenInput(DisplayValue = false)]
        public string ProblemName { get; set; }

        [Display(Name = "Тип")]
        [HiddenInput(DisplayValue = false)]
        public string SubmissionTypeName { get; set; }

        [Display(Name = "Тип")]
        [Required(ErrorMessage = "Типа е задължителен!")]
        [UIHint("DropDownList")]
        [DefaultValue(0)]
        public int SubmissionTypeId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public byte[] Content { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Съдържанието е задължителна!")]
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

        [Display(Name = "Точки")]
        [HiddenInput(DisplayValue = false)]
        [DefaultValue(0)]
        public int Points { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool Processing { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool Processed { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string Status
        {
            get
            {
                if (!this.Processing && this.Processed)
                {
                    return "Изчислен";
                }
                else if (this.Processing && !this.Processed)
                {
                    return "Изчислява се";
                }
                else if (!this.Processing && !this.Processed)
                {
                    return "Предстои изчисляване";
                }
                else
                {
                    throw new InvalidOperationException("Submission cannot be processed and processing at the same time.");
                }
            }
        }
    }
}