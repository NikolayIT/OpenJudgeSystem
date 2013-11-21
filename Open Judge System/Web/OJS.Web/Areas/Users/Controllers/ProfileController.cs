namespace OJS.Web.Areas.Users.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Controllers;
    using OJS.Web.Areas.Users.ViewModels;

    public class ProfileController : BaseController
    {
        public ProfileController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index(string id)
        {
            if (id == null)
            {
                id = this.User.Identity.Name;
            }

            var profile = this.Data.Users.GetByUsername(id);

            if (profile == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "This user does not exist!");
            }
            
            var userSettingsViewModel = new UserProfileViewModel(profile);
            userSettingsViewModel.Participations =
                this.Data.Participants.All()
                .Where(x => x.UserId == profile.Id)
                .Select(p => new UserParticipationViewModel
                {
                    ContestId = p.ContestId,
                    ContestName = p.Contest.Name,
                    IsOfficial = p.IsOfficial,
                    ContestResult = p.Submissions.GroupBy(s => s.ProblemId).Sum(x => x.Max(z => z.Points))
                });

            return this.View(userSettingsViewModel);
        }
    }
}