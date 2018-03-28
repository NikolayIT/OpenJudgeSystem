namespace OJS.Web.Areas.Api.Controllers
{
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Api.Models;
    using OJS.Web.Common;

    public class ResultsController : ApiController
    {
        private const string ErrorMessage = "ERROR";
        private const string InvalidArgumentsMessage = "Invalid arguments";
        private const string InvalidApiKeyMessage = "Invalid API key";
        private const string NoParticipantsFoundErrorMessage = ErrorMessage + ": No participants found";
        private const string MoreThanOneParticipantFoundErrorMessage = ErrorMessage + ": More than one participants found";

        private readonly IOjsData data;
        private readonly UserManager<UserProfile> userManager;

        public ResultsController(IOjsData data)
        {
            this.data = data;
            this.userManager = new OjsUserManager<UserProfile>(new UserStore<UserProfile>(data.Context.DbContext));
        }

        public ContentResult GetPointsByAnswer(string apiKey, int? contestId, string answer)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(answer) || !contestId.HasValue)
            {
                return this.Content($"{ErrorMessage}: {InvalidArgumentsMessage}");
            }

            if (!this.IsApiKeyValid(apiKey, contestId.Value))
            {
                return this.Content($"{ErrorMessage}: {InvalidApiKeyMessage}");
            }

            var participants = this.data.Participants
                .All()
                .Where(x => x.IsOfficial && x.ContestId == contestId.Value && x.Answers.Any(a => a.Answer == answer));

            var participant = participants.FirstOrDefault();
            if (participant == null)
            {
                return this.Content(NoParticipantsFoundErrorMessage);
            }

            if (participants.Count() > 1)
            {
                return this.Content(MoreThanOneParticipantFoundErrorMessage);
            }

            var points = this.GetParticipantPoints(participant);

            return this.Content(points.ToString(CultureInfo.InvariantCulture));
        }

        public ContentResult GetPointsByEmail(string apiKey, int? contestId, string email)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(email) || !contestId.HasValue)
            {
                return this.Content($"{ErrorMessage}: {InvalidArgumentsMessage}");
            }

            if (!this.IsApiKeyValid(apiKey, contestId.Value))
            {
                return this.Content($"{ErrorMessage}: {InvalidApiKeyMessage}");
            }

            var participants = this.data.Participants.All().Where(
                x => x.IsOfficial && x.ContestId == contestId.Value && x.User.Email == email);

            var participant = participants.FirstOrDefault();
            if (participant == null)
            {
                return this.Content(NoParticipantsFoundErrorMessage);
            }

            if (participants.Count() > 1)
            {
                return this.Content(MoreThanOneParticipantFoundErrorMessage);
            }

            var points = this.GetParticipantPoints(participant);

            return this.Content(points.ToString(CultureInfo.InvariantCulture));
        }

        public JsonResult GetAllResultsForContest(string apiKey, int? contestId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel(InvalidArgumentsMessage), JsonRequestBehavior.AllowGet);
            }

            if (!this.IsApiKeyValid(apiKey, contestId.Value))
            {
                return this.Json(new ErrorMessageViewModel(InvalidApiKeyMessage), JsonRequestBehavior.AllowGet);
            }

            var participants = this.data.Participants
                .All()
                .Where(x => x.IsOfficial && x.ContestId == contestId.Value)
                .Select(participant => new
                {
                    participant.User.UserName,
                    participant.User.Email,
                    Answer = participant.Answers.Select(answer => answer.Answer).FirstOrDefault(),
                    Points = participant.Contest.ProblemGroups
                        .SelectMany(pg => pg.Problems)
                        .Where(problem => !problem.IsDeleted)
                        .Select(problem => problem.Submissions
                            .Where(z => z.ParticipantId == participant.Id && !z.IsDeleted)
                            .OrderByDescending(z => z.Points)
                            .Select(z => z.Points)
                            .FirstOrDefault())
                        .Sum(),
                    Minutes = participant.Submissions
                        .Where(x => !x.IsDeleted)
                        .OrderByDescending(x => x.CreatedOn)
                        .Select(x => DbFunctions.DiffMinutes(participant.Contest.StartTime, x.CreatedOn))
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.Points)
                .ThenBy(x => x.Minutes)
                .ThenBy(x => x.UserName)
                .ToList();

            return this.Json(participants, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllUserResultsPercentageByForContest(string apiKey, int? contestId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel(InvalidArgumentsMessage), JsonRequestBehavior.AllowGet);
            }

            if (!this.IsApiKeyValid(apiKey, contestId.Value))
            {
                return this.Json(new ErrorMessageViewModel(InvalidApiKeyMessage), JsonRequestBehavior.AllowGet);
            }

            var contestMaxPoints = this.data.Problems
                .All()
                .Where(p => !p.IsDeleted && p.ProblemGroup.ContestId == contestId)
                .Select(p => (double)p.MaximumPoints)
                .DefaultIfEmpty(1)
                .Sum();

            var participants = this.data.Participants
                .All()
                .Where(x => x.ContestId == contestId.Value)
                .Select(participant => new
                {
                    participant.UserId,
                    ResultsInPercentages = participant
                        .Scores
                        .Where(s => !s.Problem.IsDeleted && s.Problem.ProblemGroup.ContestId == contestId.Value)
                        .Select(p => p.Points)
                        .DefaultIfEmpty(0)
                        .Sum() / contestMaxPoints * 100
                })
                .ToList()
                .GroupBy(p => p.UserId)
                .Select(pg => pg.OrderByDescending(p => p.ResultsInPercentages).FirstOrDefault());

            return this.Json(participants, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllResultsForContestWithPoints(string apiKey, int? contestId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel(InvalidArgumentsMessage), JsonRequestBehavior.AllowGet);
            }

            if (!this.IsApiKeyValid(apiKey, contestId.Value))
            {
                return this.Json(new ErrorMessageViewModel(InvalidApiKeyMessage), JsonRequestBehavior.AllowGet);
            }

            var participants = this.data.Participants
                .All()
                .Where(x => x.IsOfficial && x.ContestId == contestId.Value)
                .Select(participant => new
                {
                    participant.User.UserName,
                    participant.User.Email,
                    Answer = participant.Answers.Select(answer => answer.Answer).FirstOrDefault(),
                    Points = participant.Contest.ProblemGroups
                        .SelectMany(pg => pg.Problems)
                        .Where(problem => !problem.IsDeleted)
                        .Select(problem => problem.Submissions
                            .Where(z => z.ParticipantId == participant.Id && !z.IsDeleted)
                            .OrderByDescending(z => z.Points)
                            .Select(z => z.Points)
                            .FirstOrDefault())
                        .Sum(),
                    ExamTimeInMinutes = participant.Submissions
                        .Where(x => x.Problem.ProblemGroup.ContestId == contestId.Value && !x.IsDeleted)
                        .OrderByDescending(x => x.CreatedOn)
                        .Select(x => DbFunctions.DiffMinutes(participant.Contest.StartTime, x.CreatedOn))
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.Points)
                .ThenBy(x => x.ExamTimeInMinutes)
                .ToList();

            return this.Json(participants, JsonRequestBehavior.AllowGet);
        }

        private bool IsApiKeyValid(string apiKey, int contestId)
        {
            var userIsAdmin = this.userManager.IsInRole(apiKey, GlobalConstants.AdministratorRoleName);
            return this.data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (userIsAdmin || x.LecturerInContests.Any(y => y.ContestId == contestId)));
        }

        private int GetParticipantPoints(Participant participant) =>
            participant.Contest.ProblemGroups
                .SelectMany(pg => pg.Problems)
                .Select(problem => problem.Submissions
                    .Where(z => z.ParticipantId == participant.Id)
                    .OrderByDescending(z => z.Points)
                    .Select(z => z.Points)
                    .FirstOrDefault())
                .Sum();
    }
}