namespace OJS.Web.Areas.Contests.Controllers
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Helpers;
    using OJS.Web.Areas.Contests.Models;
    using OJS.Web.Areas.Contests.ViewModels;
    using OJS.Web.Controllers;

    public class CompeteController : BaseController
    {
        public const string CompeteUrl = "Compete";
        public const string PracticeUrl = "Practice";

        public CompeteController(IOjsData data)
            : base(data)
        {
        }

        public CompeteController(IOjsData data, UserProfile userProfile)
            : base(data, userProfile)
        {
        }

        /// <summary>
        /// Displays user compete information: tasks, send source form, ranking, submissions, ranking, etc.
        /// Users only.
        /// </summary>
        [Authorize]
        public ActionResult Index(int id, bool official)
        {
            var contest = this.Data.Contests.GetById(id);

            ValidateContest(contest, official);

            var participantFound = this.Data.Participants.Any(id, this.UserProfile.Id, official);

            var contestQuestions = contest.Questions.ToList();

            if (!participantFound)
            {
                if (!contest.ShouldShowRegistrationForm(official))
                {
                    this.Data.Participants.Add(new Participant(id, this.UserProfile.Id, official));
                    this.Data.SaveChanges();
                }
                else
                {
                    // Participant not found, the contest requires password or the contest has questions
                    // to be answered before registration. Redirect to the registration page.
                    // The registration page will take care of all security checks.
                    return this.RedirectToAction("Register", new { id, official });
                }
            }

            var participant = this.Data.Participants.GetWithContest(id, this.UserProfile.Id, official);
            var participantViewModel = new ParticipantViewModel(participant);

            this.ViewBag.CompeteType = official ? CompeteUrl : PracticeUrl;

            return this.View(participantViewModel);
        }

        /// <summary>
        /// Displays form for contest registration.
        /// Users only.
        /// </summary>
        [HttpGet, Authorize]
        public ActionResult Register(int id, bool official)
        {
            var participantFound = this.Data.Participants.Any(id, this.UserProfile.Id, official);
            if (participantFound)
            {
                // Participant exists. Redirect to index page.
                return this.RedirectToAction("Index", new { id, official });
            }

            var contest = this.Data.Contests.All().Include(x => x.Questions).FirstOrDefault(x => x.Id == id);

            ValidateContest(contest, official);

            if (contest.ShouldShowRegistrationForm(official))
            {
                var contestRegistrationModel = new ContestRegistrationViewModel(contest, official);

                return this.View(contestRegistrationModel);
            }

            var participant = new Participant(id, this.UserProfile.Id, official);
            this.Data.Participants.Add(participant);
            this.Data.SaveChanges();

            return this.RedirectToAction("Index", new { id, official });
        }

        /// <summary>
        /// Accepts form input for contest registration.
        /// Users only.
        /// </summary>
        [HttpPost, Authorize]
        public ActionResult Register(int id, bool official, ContestRegistrationModel registrationData)
        {
            // check if the user has already registered for participation and redirect him to the correct
            // action
            var participantFound = this.Data.Participants.Any(id, this.UserProfile.Id, official);

            if (participantFound)
            {
                return this.RedirectToAction("Index", new { id, official });
            }

            var contest = this.Data.Contests.GetById(id);

            ValidateContest(contest, official);

            // check if the contest is official, has a password and if the user entered the correct password
            if (official && contest.HasContestPassword && !contest.ContestPassword.Equals(registrationData.Password))
            {
                this.ModelState.AddModelError("Password", "Incorrect password!");
            }

            // check if the contest is practice, has a password and if the user entered the correct password
            if (!official && contest.HasPracticePassword && !contest.PracticePassword.Equals(registrationData.Password))
            {
                this.ModelState.AddModelError("Password", "Incorrect password!");
            }

            var questionsToAnswerCount = official ?
                contest.Questions.Where(x => x.AskOfficialParticipants).Count() :
                contest.Questions.Where(x => x.AskPracticeParticipants).Count();

            if (questionsToAnswerCount != registrationData.Questions.Count())
            {
                // TODO: add an appropriate model error
                this.ModelState.AddModelError(string.Empty, string.Empty);
            }

            registrationData.Questions.Each((x, i) =>
            {
                if (string.IsNullOrEmpty(x.Answer))
                {
                    this.ModelState.AddModelError(string.Format("Questions[{0}].Answer", i), "Answer is required");
                }
            });

            if (!ModelState.IsValid)
            {
                return this.View(new ContestRegistrationViewModel(contest, registrationData, official));
            }

            registrationData.Questions.Each(q =>
            {
                var contestQuestion = contest.Questions.FirstOrDefault(x => x.Id == q.QuestionId);

                contestQuestion.Answers.Add(new ContestQuestionAnswer
                {
                    QuestionId = q.QuestionId,
                    Text = q.Answer
                });
            });

            var participant = new Participant(id, this.UserProfile.Id, official);
            this.Data.Participants.Add(participant);
            this.Data.SaveChanges();

            return this.RedirectToAction("Index", new { id, official });
        }

        /// <summary>
        /// Processes a participant's submision for a problem.
        /// </summary>
        /// <param name="participantSubmission">Participant submission.</param>
        /// <param name="id">Contest id.</param>
        /// <param name="official">A check whether the contest is official or practice.</param>
        /// <returns>Returns confirmation if the submission was correctly processed.</returns>
        [HttpPost, Authorize]
        public ActionResult Submit(SubmissionModel participantSubmission, int id, bool official)
        {
            var participant = this.Data.Participants.GetWithContest(id, this.UserProfile.Id, official);

            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "You are not registered for this exam!");
            }

            ValidateContest(participant.Contest, official);

            if (participant.Submissions.Any())
            {
                // check if the submission was sent after the submission time limit has passed
                var latestSubmissionTime = participant.Submissions.Max(x => x.CreatedOn);
                TimeSpan differenceBetweenSubmissions = DateTime.Now - latestSubmissionTime;
                int limitBetweenSubmissions = participant.Contest.LimitBetweenSubmissions;

                if (differenceBetweenSubmissions.TotalSeconds < limitBetweenSubmissions)
                {
                    throw new HttpException((int)HttpStatusCode.ServiceUnavailable, "Submission was sent too soon!");
                }
            }

            if (!ModelState.IsValid)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Invalid request!");
            }

            this.Data.Submissions.Add(new Submission
            {
                ContentAsString = participantSubmission.Content,
                ProblemId = participantSubmission.ProblemId,
                SubmissionTypeId = participantSubmission.SubmissionTypeId,
                ParticipantId = participant.Id
            });

            this.Data.SaveChanges();

            return this.Json(participantSubmission.ProblemId);
        }

        /// <summary>
        /// Obtains the partial view for a particular problem.
        /// </summary>
        /// <param name="id">The problem Id</param>
        /// <param name="official">A check whether the problem is practiced or competed.</param>
        /// <returns>Returns a partial view with the problem information.</returns>
        [Authorize]
        public ActionResult Problem(int id, bool official)
        {
            this.ViewBag.CompeteType = official ? CompeteUrl : PracticeUrl;

            var problem = this.Data.Problems.GetById(id);

            if (problem == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Problem not found!");
            }

            ValidateContest(problem.Contest, official);

            if (!this.Data.Participants.Any(problem.ContestId, this.UserProfile.Id, official))
            {
                return this.RedirectToAction("Register", new { id = problem.ContestId, official });
            }

            var problemViewModel = new ContestProblemViewModel(problem);

            return this.PartialView("_ProblemPartial", problemViewModel);
        }

        /// <summary>
        /// Gets a participant's submission results for a problem.
        /// </summary>
        /// <param name="request">The Kendo data source request.</param>
        /// <param name="id">The problem id.</param>
        /// <returns>Returns the submissions results for a participant's problem.</returns>
        [Authorize]
        public ActionResult ReadSubmissionResults([DataSourceRequest]DataSourceRequest request, int id, bool official)
        {
            var problem = this.Data.Problems.GetById(id);
            var participant = this.Data.Participants.GetWithContest(problem.ContestId, this.UserProfile.Id, official);

            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "You are not registered for this exam");
            }

            var userSubmissions = this.Data.Submissions.All()
                                                        .Where(x =>
                                                                x.ProblemId == id &&
                                                                x.ParticipantId == participant.Id)
                                                        .Select(SubmissionResultViewModel.FromSubmission);

            return this.Json(userSubmissions.ToDataSourceResult(request));
        }

        /// <summary>
        /// Gets the allowed submission types for a contest.
        /// </summary>
        /// <param name="id">The contest id.</param>
        /// <returns>Returns the allowed submission types as JSON.</returns>
        public ActionResult GetAllowedSubmissionTypes(int id)
        {
            var contest = this.Data.Contests.GetById(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "The contest was not found");
            }

            var submissionTypesSelectListItems = contest
                                                    .SubmissionTypes
                                                    .ToList()
                                                    .Select(x => new SelectListItem
                                                    {
                                                        Text = x.Name,
                                                        Value = x.Id.ToString(),
                                                        Selected = x.IsSelectedByDefault
                                                    });

            return this.Json(submissionTypesSelectListItems, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets a problem resource and sends it to the user. If the user is not logged in redirects him to the 
        /// login page. If the user is not registered for the exam - redirects him to the appropriate page.
        /// </summary>
        /// <param name="id">The resource id.</param>
        /// <param name="official">A check whether the problem is practiced or competed.</param>
        /// <returns>Returns a file with the resource contents or redirects the user to the appropriate
        /// registration page.</returns>
        public ActionResult DownloadResource(int id, bool official)
        {
            var problemWithResource = this.Data.Problems
                                                    .All()
                                                    .FirstOrDefault(problem =>
                                                            problem.Resources.Any(res =>
                                                                res.Id == id));

            if (problemWithResource == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid problem!");
            }

            ValidateContest(problemWithResource.Contest, official);

            if (!problemWithResource.Contest.CanBePracticed ||
                problemWithResource.Contest.HasPracticePassword)
            {
                if (this.UserProfile == null)
                {
                    return this.RedirectToAction("Login", "Account", new { area = string.Empty });
                }

                var participant = this.Data.Participants.GetWithContest(problemWithResource.ContestId, this.UserProfile.Id, official);

                if (participant == null)
                {
                    return this.RedirectToAction("Register", new { id = problemWithResource.ContestId, official });
                }
            }

            var resource = problemWithResource.Resources.FirstOrDefault(res => res.Id == id);

            if (string.IsNullOrWhiteSpace(resource.FileExtension) || resource.File == null || resource.File.Length == 0)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "This resource cannot be downloaded!");
            }

            return this.File(resource.File, "application/octet-stream", string.Format("{0}.{1}", resource.Problem.Name, resource.FileExtension));
        }

        /// <summary>
        /// Gets the content of a participant submission for a particular problem.
        /// </summary>
        /// <param name="id">The submission id.</param>
        /// <param name="official">A flag checking if the submission was for practice or for a competition.</param>
        /// <returns>Returns a JSON with the submission content.</returns>
        [Authorize]
        public ActionResult GetSubmissionContent(int id)
        {
            var submission = this.Data.Submissions.All().FirstOrDefault(x => x.Id == id);

            if (submission == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid submission requested!");
            }

            if (submission.Participant.UserId != this.UserProfile.Id)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "This submission was not made by you!");
            }

            var contentString = submission.ContentAsString;

            return this.Json(contentString, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the results for a particular problem for users with at least one submission.
        /// </summary>
        /// <param name="request">The datasource request.</param>
        /// <param name="id">The id of the problem.</param>
        /// <param name="official">A flag checking if the requested results are for practice or for a competition.</param>
        /// <returns>Returns the best result for each user who has at least one submission for the problem.</returns>
        //// TODO: Add caching (maybe 10-20 sec). Update action so that it can be cacheable - 
        //// the entire list of results should be returned at once.
        [Authorize]
        public ActionResult GetResults([DataSourceRequest]DataSourceRequest request, int id, bool official)
        {
            var problem = this.Data.Problems.GetById(id);

            var participant = this.Data.Participants.GetWithContest(problem.ContestId, this.UserProfile.Id, official);

            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "You are not registered for this exam!");
            }

            var results = problem
                                .Submissions
                                .Where(x => x.Participant.IsOfficial == official)
                                .GroupBy(x => x.Participant)
                                .Select(grouping => new ProblemResultViewModel
                                                        {
                                                            Result = problem.Tests.Any() ? (int)Math.Round((grouping.Max(z => z.CorrectTestRunsCount) * problem.MaximumPoints) / (double)problem.Tests.Count) : 0,
                                                            ParticipantName = grouping.Key.User.UserName,
                                                            MaximumPoints = problem.MaximumPoints
                                                        })
                                .OrderBy(x => x.Result);

            return this.Json(results.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        // TODO: [Authorize(Roles = "Administrator")] // TODO: Unit test
        public ActionResult FullResults(int id, bool official)
        {
            var contest = this.Data.Contests.GetById(id);

            return this.View(contest);
        }

        public ActionResult Results(int id, bool official)
        {
            var contest = this.Data.Contests.GetById(id);

            // if the results are not visible and the participant is not registered for the contest
            // then he is not authorized to view the results
            if (!contest.ResultsArePubliclyVisible &&
                !this.Data.Participants.Any(id, this.UserProfile.Id, official))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "The results for this contest are not available");
            }

            var contestModel = new ContestResultsViewModel(contest, official);

            return this.View(contestModel);
        }

        /// <summary>
        /// Validates if a contest is correctly found. If the user wants to practice or compete in the contest
        /// checks if the contest can be practiced or competed.
        /// </summary>
        /// <param name="contest">Contest to validate.</param>
        /// <param name="official">A flag checking if the contest will be practiced or competed</param>
        [NonAction]
        public static void ValidateContest(Contest contest, bool official)
        {
            if (contest == null || contest.IsDeleted || !contest.IsVisible)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid contest id was provided!");
            }

            if (official && !contest.CanBeCompeted)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "This contest cannot be competed!");
            }

            if (!official && !contest.CanBePracticed)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "This contest cannot be practiced!");
            }
        }
    }
}
