namespace OJS.Web.Areas.Administration.ViewModels.Submission
{
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    public class SubmissionAdministrationGridViewModel : AdministrationViewModel
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
                    ProblemName = sub.Problem.Name,
                    SubmissionTypeName = sub.SubmissionType.Name,
                    Points = sub.Points,
                    Processed = sub.Processed,
                    Processing = sub.Processing,
                    CreatedOn = sub.CreatedOn,
                    ModifiedOn = sub.ModifiedOn,
                };
            }
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [Display(Name = "Задача")]
        public string ProblemName { get; set; }

        [Display(Name = "Потребител")]
        public string ParticipantName { get; set; }

        [Display(Name = "Тип")]
        public string SubmissionTypeName { get; set; }

        [Display(Name = "Точки")]
        public int? Points { get; set; }

        [ScaffoldColumn(false)]
        public bool Processing { get; set; }

        [ScaffoldColumn(false)]
        public bool Processed { get; set; }

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