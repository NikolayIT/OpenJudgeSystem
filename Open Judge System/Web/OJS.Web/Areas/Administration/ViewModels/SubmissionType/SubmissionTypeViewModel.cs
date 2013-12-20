namespace OJS.Web.Areas.Administration.ViewModels.SubmissionType
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using Newtonsoft.Json;

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
    }
}