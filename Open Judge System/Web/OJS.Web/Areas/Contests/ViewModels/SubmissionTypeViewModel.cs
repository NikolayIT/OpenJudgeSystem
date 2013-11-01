namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class SubmissionTypeViewModel
    {
        public static Expression<Func<SubmissionType, SubmissionTypeViewModel>> FromSubmissionType
        {
            get
            {
                return submission => new SubmissionTypeViewModel
                                                            {
                                                                Id = submission.Id,
                                                                Name = submission.Name
                                                            };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }
    }
}