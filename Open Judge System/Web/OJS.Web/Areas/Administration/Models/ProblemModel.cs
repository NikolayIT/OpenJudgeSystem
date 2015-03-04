namespace OJS.Web.Areas.Administration.Models
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ProblemModel
    {
        public static Expression<Func<Problem, ProblemModel>> Model
        {
            get
            {
                return p => new ProblemModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    MaximumPoints = p.MaximumPoints,
                    TimeLimit = p.TimeLimit,
                    MemoryLimit = p.MemoryLimit
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public short MaximumPoints { get; set; }

        public int TimeLimit { get; set; }

        public int MemoryLimit { get; set; }
    }
}