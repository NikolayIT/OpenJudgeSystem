namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Areas.Administration.ViewModels.AntiCheat;
    using OJS.Web.Controllers;

    public class AntiCheatController : AdministrationController
    {
        public AntiCheatController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult ByIP()
        {
            var contests = this.Data.Contests
                .All()
                .OrderByDescending(c => c.CreatedOn)
                .Select(c =>
                    new
                    {
                        Text = c.Name,
                        Value = c.Id
                    })
                .ToList()
                .Select(c =>
                    new SelectListItem
                    {
                        Text = c.Text,
                        Value = c.Value.ToString()
                    });

            return this.View(contests);
        }

        public ActionResult RenderByIpGrid(int id, string excludeIps)
        {
            var renderByIpAddress = this.Data.Participants
                .All()
                .Where(p => p.ContestId == id && p.IsOfficial)
                .Select(AntiCheatByIpAdministrationViewModel.ViewModel)
                .Where(p => p.DifferentIps.Count() > 1)
                .ToList();

            if (!string.IsNullOrEmpty(excludeIps))
            {
                var withoutExcludeIps = this.Data.Participants.All();

                var ipsToExclude = excludeIps.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var ip in ipsToExclude)
                {
                    withoutExcludeIps = withoutExcludeIps.Where(
                        p => p.ContestId == id 
                        && p.IsOfficial 
                        && p.Submissions.AsQueryable().Count() > 1 
                        && p.Submissions.AsQueryable()
                            .Where(s => !s.IsDeleted && s.IpAddress != null)
                            .All(s => s.IpAddress != ip));
                }

                renderByIpAddress.AddRange(withoutExcludeIps.Select(AntiCheatByIpAdministrationViewModel.ViewModel));
            }

            return this.PartialView("_IPGrid", renderByIpAddress);
        }
    }
}