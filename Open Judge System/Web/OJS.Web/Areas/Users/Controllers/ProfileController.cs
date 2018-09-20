namespace OJS.Web.Areas.Users.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.Users;
    using OJS.Web.Areas.Users.ViewModels;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Users.Views.Profile;

    public class ProfileController : BaseController
    {
        private readonly IParticipantsDataService participantsData;
        private readonly IUsersDataService usersData;

        public ProfileController(
            IOjsData data,
            IParticipantsDataService participantsData,
            IUsersDataService usersData)
            : base(data)
        {
            this.participantsData = participantsData;
            this.usersData = usersData;
        }

        public ActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = this.User.Identity.Name;
            }

            var profile = this.usersData.GetByUsername(id);

            if (profile == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.ProfileIndex.Not_found);
            }

            var userSettingsViewModel = new UserProfileViewModel(profile)
            {
                Participations = this.participantsData
                    .GetAllByUser(profile.Id)
                    .GroupBy(x => x.Contest)
                    .Select(c => new UserParticipationViewModel
                    {
                        ContestId = c.Key.Id,
                        ContestName = c.Key.Name,
                        RegistrationTime = c.Key.CreatedOn,
                        ContestCompeteMaximumPoints = c.Key.ProblemGroups
                            .Where(pg => pg.Problems.Any(p => !p.IsDeleted))
                            .Sum(pg => pg.Problems.FirstOrDefault().MaximumPoints),
                        ContestPracticeMaximumPoints = c.Key.ProblemGroups
                            .SelectMany(pg => pg.Problems)
                            .Where(x => !x.IsDeleted)
                            .Sum(pr => pr.MaximumPoints),
                        CompeteResult = c.Where(x => x.IsOfficial)
                            .Select(p => p.Submissions
                                .Where(x => !x.IsDeleted)
                                .GroupBy(s => s.ProblemId)
                                .Sum(x => x.Max(z => z.Points)))
                            .FirstOrDefault(),
                        PracticeResult = c.Where(x => !x.IsOfficial)
                            .Select(p => p.Submissions
                                .Where(x => !x.IsDeleted)
                                .GroupBy(s => s.ProblemId)
                                .Sum(x => x.Max(z => z.Points)))
                            .FirstOrDefault()
                    })
                    .OrderByDescending(x => x.RegistrationTime)
                    .ToList()
            };

            return this.View(userSettingsViewModel);
        }
    }
}