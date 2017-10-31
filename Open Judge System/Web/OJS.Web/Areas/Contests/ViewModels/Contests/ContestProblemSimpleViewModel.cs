namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System;
    using System.Linq.Expressions;
    using OJS.Data.Models;

    public class ContestProblemSimpleViewModel
    {
        public static Expression<Func<Problem, ContestProblemSimpleViewModel>> FromProblem =>
            pr => new ContestProblemSimpleViewModel
                {
                    Name = pr.Name,
                    ShowResults = pr.ShowResults,
                    MaximumPoints = pr.MaximumPoints
                };

        public string Name { get; set; }
        
        public bool ShowResults { get; set; }

        public short MaximumPoints { get; set; }
    }
}