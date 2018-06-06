namespace OJS.Web.Areas.Api.Controllers
{
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Api.Models;
    using OJS.Web.Infrastructure.Filters.Attributes;

    [ValidateRemoteDataApiKey]
    public class ResultsController : ApiController
    {
        private const string ErrorMessage = "ERROR";
        private const string InvalidArgumentsMessage = "Invalid arguments";
        private const string NoParticipantsFoundErrorMessage = ErrorMessage + ": No participants found";
        private const string MoreThanOneParticipantFoundErrorMessage = ErrorMessage + ": More than one participants found";

        private readonly IOjsData data;

        public ResultsController(IOjsData data)
        {
            this.data = data;
        }

        public ContentResult GetPointsByAnswer(string apiKey, int? contestId, string answer)
        {
            if (string.IsNullOrWhiteSpace(answer) || !contestId.HasValue)
            {
                return this.Content($"{ErrorMessage}: {InvalidArgumentsMessage}");
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
            if (string.IsNullOrWhiteSpace(email) || !contestId.HasValue)
            {
                return this.Content($"{ErrorMessage}: {InvalidArgumentsMessage}");
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
            if (!contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel(InvalidArgumentsMessage), JsonRequestBehavior.AllowGet);
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
            if (!contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel(InvalidArgumentsMessage), JsonRequestBehavior.AllowGet);
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
            if (!contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel(InvalidArgumentsMessage), JsonRequestBehavior.AllowGet);
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