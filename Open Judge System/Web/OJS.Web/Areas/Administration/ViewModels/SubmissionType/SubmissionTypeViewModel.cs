namespace OJS.Web.Areas.Administration.ViewModels.SubmissionType
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using Newtonsoft.Json;

    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Contest;

    public class SubmissionTypeViewModel
    {
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

        [JsonIgnore]
        public SubmissionType ToEntity
        {
            get
            {
                return new SubmissionType
                {
                    Id = this.Id ?? default(int),
                };
            }
        }

        public int? Id { get; set; }

        public string Name { get; set; }

        public bool IsChecked { get; set; }

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
    }
}