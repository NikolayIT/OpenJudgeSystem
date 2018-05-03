namespace OJS.Web.Areas.Administration.ViewModels.SubmissionType
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Problem;

    public class SubmissionTypeViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<SubmissionType, SubmissionTypeViewModel>> ViewModel
        {
            get
            {
                return st => new SubmissionTypeViewModel
                {
                    Id = st.Id,
                    Name = st.Name
                };
            }
        }

        public int? Id { get; set; }

        public string Name { get; set; }

        public bool IsChecked { get; set; }

        public static Action<SubmissionTypeViewModel> ApplySelectedTo(ProblemAdministrationViewModel problem)
        {
            return st =>
            {
                var submissionViewModel = new SubmissionTypeViewModel
                {
                    Id = st.Id,
                    Name = st.Name,
                    IsChecked = false,
                };

                var selectedSubmission = problem.SelectedSubmissionTypes.FirstOrDefault(s => s.Id == st.Id);

                if (selectedSubmission != null)
                {
                    submissionViewModel.IsChecked = true;
                }

                problem.SubmissionTypes.Add(submissionViewModel);
            };
        }

        public SubmissionType GetEntityModel(SubmissionType model = null)
        {
            model = model ?? new SubmissionType();
            model.Id = this.Id.Value;
            return model;
        }
    }
}