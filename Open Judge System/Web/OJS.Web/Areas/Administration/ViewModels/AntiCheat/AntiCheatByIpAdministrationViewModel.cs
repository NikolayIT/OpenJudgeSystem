namespace OJS.Web.Areas.Administration.ViewModels.AntiCheat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class AntiCheatByIpAdministrationViewModel
    {
        public static Expression<Func<Participant, AntiCheatByIpAdministrationViewModel>> ViewModel
        {
            get
            {
                return p => new AntiCheatByIpAdministrationViewModel
                    {
                        Id = p.Id,
                        UserName = p.User.UserName,
                        Points = p.Submissions
                            .AsQueryable()
                            .Where(s => !s.IsDeleted)
                            .GroupBy(s => s.ProblemId)
                            .Sum(s => s.Max(z => z.Points)),
                        DifferentIps = p.Submissions
                            .AsQueryable()
                            .Where(s => !s.IsDeleted && s.IpAddress != null)
                            .GroupBy(s => s.IpAddress)
                            .Select(g => g.FirstOrDefault().IpAddress)
                    };
            }
        }

        public int Id { get; set; }

        public string UserName { get; set; }

        public int Points { get; set; }

        public IEnumerable<string> DifferentIps { get; set; }
    }
}