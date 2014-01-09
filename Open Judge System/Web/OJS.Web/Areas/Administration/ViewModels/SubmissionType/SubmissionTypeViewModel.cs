namespace OJS.Web.Areas.Administration.ViewModels.SubmissionType
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using Newtonsoft.Json;

    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Common.DataAnnotations;

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

        public static Action<SubmissionTypeViewModel> ApplySelectedTo(ContestAdministrationViewModel contest)
        {
            return st =>
            {
                var submissionViewModel = new SubmissionTypeViewModel
                {
                    Id = st.Id,
                    Name = st.Name,
                    IsChecked = false,
                };

                var selectedSubmission = contest.SelectedSubmissionTypes.FirstOrDefault(s => s.Id == st.Id);

                if (selectedSubmission != null)
                {
                    submissionViewModel.IsChecked = true;
                }

                contest.SubmisstionTypes.Add(submissionViewModel);
            };
        }

        public int? Id { get; set; }

        public string Name { get; set; }

        public bool IsChecked { get; set; }

        public SubmissionType GetEntityModel(SubmissionType model = null)
        {
            model = model ?? new SubmissionType();
            model.Id = this.Id.Value;
            return model;
        }
    }
}