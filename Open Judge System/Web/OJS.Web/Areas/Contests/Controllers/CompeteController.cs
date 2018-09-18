namespace OJS.Web.Areas.Contests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Expressions;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Business.Contests;
    using OJS.Services.Business.Participants;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Ips;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.Problems;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Web.Areas.Contests.Helpers;
    using OJS.Web.Areas.Contests.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Participants;
    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Common.Exceptions;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests;

    public class CompeteController : BaseController
    {
        public const string CompeteActionName = "Compete";
        public const string PracticeActionName = "Practice";

        private readonly IParticipantsBusinessService participantsBusiness;
        private readonly IContestsBusinessService contestsBusiness;
        private readonly IParticipantsDataService participantsData;
        private readonly IContestsDataService contestsData;
        private readonly IProblemsDataService problemsData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly ISubmissionsForProcessingDataService submissionsForProcessingData;
        private readonly IIpsDataService ipsData;

        public CompeteController(
            IOjsData data,
            IParticipantsBusinessService participantsBusiness,
            IContestsBusinessService contestsBusiness,
            IParticipantsDataService participantsData,
            IContestsDataService contestsData,
            IProblemsDataService problemsData,
            ISubmissionsDataService submissionsData,
            ISubmissionsForProcessingDataService submissionsForProcessingData,
            IIpsDataService ipsData)
            : base(data)
        {
            this.participantsBusiness = participantsBusiness;
            this.contestsBusiness = contestsBusiness;
            this.participantsData = participantsData;
            this.contestsData = contestsData;
            this.problemsData = problemsData;
            this.submissionsData = submissionsData;
            this.submissionsForProcessingData = submissionsForProcessingData;
            this.ipsData = ipsData;
        }

        protected CompeteController(
            IOjsData data,
            UserProfile userProfile)
            : base(data, userProfile)
        {
        }

        /// <summary>
        /// Validates if the selected submission type from the participant is allowed in the current problem
        /// </summary>
        /// <param name="submissionTypeId">The id of the submission type selected by the participant</param>
        /// <param name="problem">The problem which the user is attempting to solve</param>
        /// <param name="shouldAllowBinaryFiles">Bool indicating if the validation should allow binary file uploads</param>
        [NonAction]
        public static SubmissionType ValidateSubmissionType(int submissionTypeId, Problem problem, bool shouldAllowBinaryFiles)
        {
            var submissionType = problem.SubmissionTypes.FirstOrDefault(st => st.Id == submissionTypeId);
            if (submissionType == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Submission_type_not_found);
            }

            if (shouldAllowBinaryFiles && !submissionType.AllowBinaryFilesUpload)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Binary_files_not_allowed);
            }

            if (!shouldAllowBinaryFiles && submissionType.AllowBinaryFilesUpload)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Text_upload_not_allowed);
            }

            return submissionType;
        }

        /// <summary>
        /// Validates if a contest is correctly found. If the user wants to practice or compete in the contest
        /// checks if the contest can be practiced or competed.
        /// </summary>
        /// <param name="contest">Contest to validate.</param>
        /// <param name="official">A flag checking if the contest will be practiced or competed</param>
        [NonAction]
        public void ValidateContest(Contest contest, bool official)
        {
            var isUserAdminOrLecturerInContest = this.IsUserAdminOrLecturerInContest(contest);

            if (contest == null ||
                contest.IsDeleted ||
                (!contest.IsVisible && !isUserAdminOrLecturerInContest))
            {
                throw new HttpException(
                    (int)HttpStatusCode.NotFound,
                    Resource.ContestsGeneral.Contest_not_found);
            }

            if (official &&
                !this.contestsBusiness.CanUserCompeteByContestByUserAndIsAdmin(
                    contest.Id,
                    this.UserProfile.Id,
                    this.User.IsAdmin(),
                    allowToAdminAlways: true))
            {
                throw new ContestCannotBeCompetedException(
                    (int)HttpStatusCode.Forbidden,
                    Resource.ContestsGeneral.Contest_cannot_be_competed);
            }

            if (!official && !contest.CanBePracticed && !isUserAdminOrLecturerInContest)
            {
                throw new HttpException(
                    (int)HttpStatusCode.Forbidden,
                    Resource.ContestsGeneral.Contest_cannot_be_practiced);
            }
        }

        /// <summary>
        /// Displays user compete information: tasks, send source form, ranking, submissions, ranking, etc.
        /// Users only.
        /// </summary>
        [Authorize]
        [DisableCache]
        public ActionResult Index(int id, bool official, bool? hasConfirmed)
        {
            var contest = this.contestsData.GetById(id);

            try
            {
                this.ValidateContest(contest, official);
            }
            catch (HttpException httpEx)
            {
                if (httpEx is ContestCannotBeCompetedException && contest.CanBePracticed)
                {
                    return this.RedirectToAction(c => c.Index(id, false, null));
                }

                this.TempData.AddDangerMessage(httpEx.Message);
                return this.RedirectToHome();
            }

            var isUserAdminOrLecturerInContest = this.IsUserAdminOrLecturerInContest(contest);

            var participant = this.participantsData
                .GetWithContestByContestByUserAndIsOfficial(id, this.UserProfile.Id, official);

            if (participant == null || participant.IsInvalidated)
            {
                var shouldShowConfirmation = participant == null &&
                    official &&
                    contest.IsOnline &&
                    (!hasConfirmed.HasValue || hasConfirmed.Value == false) &&
                    contest.Duration.HasValue &&
                    !isUserAdminOrLecturerInContest;

                if (!shouldShowConfirmation)
                {
                    return this.RedirectToAction(c => c.Register(id, official));
                }

                var confirmViewModel = new OnlineContestConfirmViewModel
                {
                    ContesId = contest.Id,
                    ContestName = contest.Name,
                    ContestDuration = contest.Duration.Value,
                    ProblemGroupsCount = contest.ProblemGroups.Count(pg => !pg.IsDeleted)
                };

                return this.View("ConfirmCompete", confirmViewModel);
            }

            if (official &&
                !this.contestsBusiness.IsContestIpValidByContestAndIp(id, this.Request.UserHostAddress))
            {
                return this.RedirectToAction(c => c.NewContestIp(id));
            }

            var participantViewModel = new ParticipantViewModel(
                participant,
                official,
                isUserAdminOrLecturerInContest);

            this.ViewBag.CompeteType = official ? CompeteActionName : PracticeActionName;
            this.ViewBag.IsUserAdminOrLecturer = isUserAdminOrLecturerInContest;

            return this.View(participantViewModel);
        }

        /// <summary>
        /// Displays form for contest registration.
        /// Users only.
        /// </summary>
        [HttpGet]
        [Authorize]
        public ActionResult Register(int id, bool official)
        {
            var participant = this.participantsData
                .GetByContestByUserAndByIsOfficial(id, this.UserProfile.Id, official);

            if (participant != null && !participant.IsInvalidated)
            {
                return this.RedirectToAction(GlobalConstants.Index, new { id, official });
            }

            var contest = this.contestsData.GetById(id);

            try
            {
                this.ValidateContest(contest, official);

                if (contest.ShouldShowRegistrationForm(official))
                {
                    var contestRegistrationModel = new ContestRegistrationViewModel(contest, official);
                    return this.View(contestRegistrationModel);
                }

                if (participant == null)
                {
                    this.AddNewParticipantToContest(contest, official);
                }
                else
                {
                    participant.IsInvalidated = false;
                    this.participantsData.Update(participant);
                }
            }
            catch (HttpException httpEx)
            {
                this.TempData.AddDangerMessage(httpEx.Message);
                return this.RedirectToHome();
            }

            return this.RedirectToAction(GlobalConstants.Index, new { id, official });
        }

        /// <summary>
        /// Accepts form input for contest registration.
        /// Users only.
        /// </summary>
        //// TODO: Refactor
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Register(bool official, ContestRegistrationModel registrationData)
        {
            var participant = this.participantsData.GetByContestByUserAndByIsOfficial(registrationData.ContestId, this.UserProfile.Id, official);

            if (participant != null && !participant.IsInvalidated)
            {
                return this.RedirectToAction(GlobalConstants.Index, new { id = registrationData.ContestId, official });
            }

            var contest = this.contestsData.GetById(registrationData.ContestId);
            try
            {
                this.ValidateContest(contest, official);

                if (official && contest.HasContestPassword)
                {
                    if (string.IsNullOrEmpty(registrationData.Password))
                    {
                        this.ModelState.AddModelError("Password", Resource.Views.CompeteRegister.Empty_Password);
                    }
                    else if (contest.ContestPassword != registrationData.Password)
                    {
                        this.ModelState.AddModelError("Password", Resource.Views.CompeteRegister.Incorrect_password);
                    }
                }

                if (!official && contest.HasPracticePassword)
                {
                    if (string.IsNullOrEmpty(registrationData.Password))
                    {
                        this.ModelState.AddModelError("Password", Resource.Views.CompeteRegister.Empty_Password);
                    }
                    else if (contest.PracticePassword != registrationData.Password)
                    {
                        this.ModelState.AddModelError("Password", Resource.Views.CompeteRegister.Incorrect_password);
                    }
                }

                var questionsToAnswerCount = official ?
                    contest.Questions.Count(x => !x.IsDeleted && x.AskOfficialParticipants) :
                    contest.Questions.Count(x => !x.IsDeleted && x.AskPracticeParticipants);

                if (questionsToAnswerCount != registrationData.Questions.Count())
                {
                    this.ModelState.AddModelError("Questions", Resource.Views.CompeteRegister.Not_all_questions_answered);
                }

                var contestQuestions = contest.Questions.Where(x => !x.IsDeleted).ToList();

                var counter = 0;
                var answers = new List<ParticipantAnswer>();
                foreach (var question in registrationData.Questions)
                {
                    var contestQuestion = contestQuestions.FirstOrDefault(x => x.Id == question.QuestionId);

                    var regularExpression = contestQuestion.RegularExpressionValidation;
                    var correctlyAnswered = false;

                    if (!string.IsNullOrEmpty(regularExpression))
                    {
                        correctlyAnswered = Regex.IsMatch(question.Answer, regularExpression);
                    }

                    if (contestQuestion.Type == ContestQuestionType.DropDown)
                    {
                        if (int.TryParse(question.Answer, out var contestAnswerId) &&
                            contestQuestion.Answers.Where(x => !x.IsDeleted).Any(x => x.Id == contestAnswerId))
                        {
                            correctlyAnswered = true;
                        }

                        if (!correctlyAnswered)
                        {
                            this.ModelState.AddModelError(
                                $"Questions[{counter}].Answer",
                                Resource.ContestsGeneral.Invalid_selection);
                        }
                    }

                    answers.Add(
                        new ParticipantAnswer
                        {
                            ContestQuestionId = question.QuestionId,
                            Answer = question.Answer
                        });

                    counter++;
                }

                if (!this.ModelState.IsValid)
                {
                    return this.View(new ContestRegistrationViewModel(contest, registrationData, official));
                }

                if (participant == null)
                {
                    participant = this.AddNewParticipantToContest(contest, official);
                }
                else
                {
                    participant.IsInvalidated = false;
                }

                foreach (var participantAnswer in answers)
                {
                    participant.Answers.Add(participantAnswer);
                }

                this.participantsData.Update(participant);
            }
            catch (HttpException httpEx)
            {
                this.TempData.AddDangerMessage(httpEx.Message);
                return this.RedirectToHome();
            }

            return this.RedirectToAction(GlobalConstants.Index, new { id = registrationData.ContestId, official });
        }

        /// <summary>
        /// Processes a participant's submission for a problem.
        /// </summary>
        /// <param name="participantSubmission">Participant submission.</param>
        /// <param name="official">A check whether the contest is official or practice.</param>
        /// <returns>Returns confirmation if the submission was correctly processed.</returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(SubmissionModel participantSubmission, bool official)
        {
            var problem = this.problemsData.GetWithProblemGroupById(participantSubmission.ProblemId);
            if (problem == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.Problem_not_found);
            }

            var participant = this.participantsData
                .GetWithContestByContestByUserAndIsOfficial(problem.ProblemGroup.ContestId, this.UserProfile.Id, official);
            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.User_is_not_registered_for_exam);
            }

            this.ValidateContest(participant.Contest, official);

            this.ValidateProblemForParticipant(
                participant,
                participant.Contest,
                participantSubmission.ProblemId,
                official);

            if (official &&
                !this.contestsBusiness.IsContestIpValidByContestAndIp(problem.ProblemGroup.ContestId, this.Request.UserHostAddress))
            {
                return this.RedirectToAction("NewContestIp", new { id = problem.ProblemGroup.ContestId });
            }

            ValidateSubmissionType(participantSubmission.SubmissionTypeId, problem, shouldAllowBinaryFiles: false);

            if (!this.ModelState.IsValid)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Invalid_request);
            }

            if (this.Data.Submissions.HasSubmissionTimeLimitPassedForParticipant(participant.Id, participant.Contest.LimitBetweenSubmissions))
            {
                throw new HttpException((int)HttpStatusCode.ServiceUnavailable, Resource.ContestsGeneral.Submission_was_sent_too_soon);
            }

            if (problem.SourceCodeSizeLimit < participantSubmission.Content.Length)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Submission_too_long);
            }

            if (this.Data.Submissions.HasUserNotProcessedSubmissionForProblem(problem.Id, this.UserProfile.Id))
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.User_has_not_processed_submission_for_problem);
            }

            var contest = participant.Contest;

            var newSubmission = new Submission
            {
                ContentAsString = participantSubmission.Content,
                ProblemId = participantSubmission.ProblemId,
                SubmissionTypeId = participantSubmission.SubmissionTypeId,
                ParticipantId = participant.Id,
                IpAddress = this.Request.UserHostAddress,
                IsPublic = ((participant.IsOfficial && contest.ContestPassword == null) ||
                                (!participant.IsOfficial && contest.PracticePassword == null)) &&
                            contest.IsVisible &&
                            !contest.IsDeleted &&
                            problem.ShowResults
            };

            this.Data.Submissions.Add(newSubmission);
            this.Data.SaveChanges();

            this.submissionsForProcessingData.AddOrUpdateBySubmission(newSubmission.Id);

            return this.Json(participantSubmission.ProblemId);
        }

        // TODO: Extract common logic between SubmitBinaryFile and Submit methods
        public ActionResult SubmitBinaryFile(BinarySubmissionModel participantSubmission, bool official, int? returnProblem)
        {
            if (participantSubmission?.File == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Upload_file);
            }

            var problem = this.problemsData.GetWithProblemGroupById(participantSubmission.ProblemId);
            if (problem == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.Problem_not_found);
            }

            var participant = this.participantsData
                .GetWithContestByContestByUserAndIsOfficial(problem.ProblemGroup.ContestId, this.UserProfile.Id, official);
            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.User_is_not_registered_for_exam);
            }

            this.ValidateContest(participant.Contest, official);

            this.ValidateProblemForParticipant(
                participant,
                participant.Contest,
                participantSubmission.ProblemId,
                official);

            if (official &&
                !this.contestsBusiness.IsContestIpValidByContestAndIp(problem.ProblemGroup.ContestId, this.Request.UserHostAddress))
            {
                return this.RedirectToAction("NewContestIp", new { id = problem.ProblemGroup.ContestId });
            }

            if (this.Data.Submissions.HasSubmissionTimeLimitPassedForParticipant(participant.Id, participant.Contest.LimitBetweenSubmissions))
            {
                throw new HttpException((int)HttpStatusCode.ServiceUnavailable, Resource.ContestsGeneral.Submission_was_sent_too_soon);
            }

            if (problem.SourceCodeSizeLimit < participantSubmission.File.ContentLength)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Submission_too_long);
            }

            var submissionType = ValidateSubmissionType(
                participantSubmission.SubmissionTypeId,
                problem,
                shouldAllowBinaryFiles: true);

            // Validate file extension
            if (!submissionType.AllowedFileExtensionsList.Contains(
                participantSubmission.File.FileName.GetFileExtension()))
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Invalid_extention);
            }

            if (!this.ModelState.IsValid)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Invalid_request);
            }

            var newSubmission = new Submission
            {
                Content = participantSubmission.File.InputStream.ToByteArray(),
                FileExtension = participantSubmission.File.FileName.GetFileExtension(),
                ProblemId = participantSubmission.ProblemId,
                SubmissionTypeId = participantSubmission.SubmissionTypeId,
                ParticipantId = participant.Id
            };

            this.Data.Submissions.Add(newSubmission);
            this.Data.SaveChanges();

            this.submissionsForProcessingData.AddOrUpdateBySubmission(newSubmission.Id);

            this.TempData.Add(GlobalConstants.InfoMessage, Resource.ContestsGeneral.Solution_uploaded);
            return this.Redirect(string.Format("/Contests/{2}/Index/{0}#{1}", problem.ProblemGroup.ContestId, returnProblem ?? 0, official ? CompeteActionName : PracticeActionName));
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
            this.ViewBag.IsOfficial = official;
            this.ViewBag.CompeteType = official ? CompeteActionName : PracticeActionName;

            var problem = this.problemsData
                .GetByIdQuery(id)
                .Select(ContestProblemViewModel.FromProblem)
                .FirstOrDefault();

            if (problem == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.ContestsGeneral.Problem_not_found);
            }

            this.ValidateContest(this.contestsData.GetById(problem.ContestId), official);

            if (!this.participantsData
                .ExistsByContestByUserAndIsOfficial(problem.ContestId, this.UserProfile.Id, official))
            {
                return this.RedirectToAction("Register", new { id = problem.ContestId, official });
            }

            if (official &&
                !this.contestsBusiness.IsContestIpValidByContestAndIp(problem.ContestId, this.Request.UserHostAddress))
            {
                return this.RedirectToAction("NewContestIp", new { id = problem.ContestId });
            }

            problem.UserHasAdminRights = this.CheckIfUserHasProblemPermissions(id);

            return this.PartialView("_ProblemPartial", problem);
        }

        /// <summary>
        /// Gets a participant's submission results for a problem.
        /// </summary>
        /// <param name="request">The Kendo data source request.</param>
        /// <param name="id">The problem id.</param>
        /// <param name="official">A check whether the problem is practiced or competed.</param>
        /// <returns>Returns the submissions results for a participant's problem.</returns>
        [Authorize]
        public ActionResult ReadSubmissionResults([DataSourceRequest]DataSourceRequest request, int id, bool official)
        {
            var problem = this.problemsData.GetWithProblemGroupById(id);

            var participant = this.participantsData.GetWithContestByContestByUserAndIsOfficial(
                problem.ProblemGroup.ContestId,
                this.UserProfile.Id,
                official);

            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.User_is_not_registered_for_exam);
            }

            if (!problem.ShowResults)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Problem_results_not_available);
            }

            var userSubmissions = this.submissionsData
                .GetAllByProblemAndParticipant(id, participant.Id)
                .Select(SubmissionResultViewModel.FromSubmission);

            return this.Json(userSubmissions.ToDataSourceResult(request));
        }

        [Authorize]
        public ActionResult ReadSubmissionResultsAreCompiled([DataSourceRequest]DataSourceRequest request, int id, bool official)
        {
            var problem = this.problemsData.GetWithProblemGroupById(id);

            var participant = this.participantsData.GetWithContestByContestByUserAndIsOfficial(
                problem.ProblemGroup.ContestId,
                this.UserProfile.Id,
                official);

            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.User_is_not_registered_for_exam);
            }

            var userSubmissions = this.submissionsData
                .GetAllByProblemAndParticipant(id, participant.Id)
                .Select(SubmissionResultIsCompiledViewModel.FromSubmission);

            return this.Json(userSubmissions.ToDataSourceResult(request));
        }

        /// <summary>
        /// Gets the allowed submission types for a problem.
        /// </summary>
        /// <param name="id">The problem id.</param>
        /// <returns>Returns the allowed submission types as JSON.</returns>
        public ActionResult GetAllowedSubmissionTypes(int id)
        {
            var submissionTypesSelectListItems =
                this.Data.Problems.All()
                    .Where(x => x.Id == id)
                    .SelectMany(x => x.SubmissionTypes)
                    .ToList()
                    .Select(x => new
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(CultureInfo.InvariantCulture),
                        Selected = x.IsSelectedByDefault,
                        x.AllowBinaryFilesUpload,
                        x.AllowedFileExtensions
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
                                                            problem.Resources.Any(res => res.Id == id && !res.IsDeleted));

            if (problemWithResource == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.ContestsGeneral.Problem_not_found);
            }

            var contest = problemWithResource.ProblemGroup.Contest;
            var userCanDownloadResource = false;

            if (this.UserProfile == null)
            {
                this.ValidateContest(contest, official);
            }
            else if (this.User != null && this.User.IsAdmin())
            {
                // TODO: add unit tests
                // If the user is an administrator he can download the resource at any time.
                userCanDownloadResource = true;
            }
            else
            {
                this.ValidateContest(contest, official);
                userCanDownloadResource = this.participantsData
                    .ExistsByContestByUserAndIsOfficial(contest.Id, this.UserProfile.Id, official);
            }

            if (userCanDownloadResource ||
                (contest.CanBeCompeted && !contest.HasContestPassword) ||
                (contest.CanBePracticed && !contest.HasPracticePassword))
            {
                var resource = problemWithResource.Resources.FirstOrDefault(res => res.Id == id);

                if (string.IsNullOrWhiteSpace(resource.FileExtension) || resource.File == null || resource.File.Length == 0)
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Resource_cannot_be_downloaded);
                }

                return this.File(resource.File, GlobalConstants.BinaryFileMimeType, string.Format("{0}_{1}.{2}", resource.Problem.Name, resource.Name, resource.FileExtension));
            }

            if ((contest.CanBePracticed && !official) || (contest.CanBeCompeted && official))
            {
                return this.RedirectToAction("Register", new { official, id = contest.Id });
            }

            throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Resource_cannot_be_downloaded);
        }

        /// <summary>
        /// Gets the content of a participant submission for a particular problem.
        /// </summary>
        /// <param name="id">The submission id.</param>
        /// <returns>Returns a JSON with the submission content.</returns>
        //// TODO: Remove if not used
        [Authorize]
        public ActionResult GetSubmissionContent(int id)
        {
            var submission = this.Data.Submissions.All().FirstOrDefault(x => x.Id == id);

            if (submission == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.ContestsGeneral.Submission_not_found);
            }

            if (submission.Participant.UserId != this.UserProfile.Id)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Submission_not_made_by_user);
            }

            var contentString = submission.ContentAsString;

            return this.Json(contentString, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult NewContestIp(int id)
        {
            if (!this.participantsData.ExistsByContestByUserAndIsOfficial(id, this.UserProfile.Id, true))
            {
                return this.RedirectToAction("Register", new { id, official = true });
            }

            if (this.contestsBusiness.IsContestIpValidByContestAndIp(id, this.Request.UserHostAddress))
            {
                return this.RedirectToAction(GlobalConstants.Index, new { id, official = true });
            }

            var contest = this.contestsData.GetById(id);

            this.ValidateContest(contest, true);

            var model = new NewContestIpViewModel { ContestId = id };
            return this.View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult NewContestIp(NewContestIpViewModel model)
        {
            if (!this.participantsData.ExistsByContestByUserAndIsOfficial(model.ContestId, this.UserProfile.Id, true))
            {
                return this.RedirectToAction("Register", new { id = model.ContestId, official = true });
            }

            if (this.contestsBusiness.IsContestIpValidByContestAndIp(model.ContestId, this.Request.UserHostAddress))
            {
                return this.RedirectToAction(GlobalConstants.Index, new { id = model.ContestId, official = true });
            }

            var contest = this.contestsData
                .GetByIdQuery(model.ContestId)
                .Include(x => x.AllowedIps.Select(aIp => aIp.Ip))
                .FirstOrDefault();

            this.ValidateContest(contest, true);

            if (!string.Equals(contest.NewIpPassword, model.NewIpPassword, StringComparison.InvariantCulture))
            {
                this.ModelState.AddModelError("NewIpPassword", "Невалидна парола.");
                return this.View(model);
            }

            var currentUserIpAddress = this.Request.UserHostAddress;
            if (contest.AllowedIps.All(y => y.Ip.Value != currentUserIpAddress))
            {
                var ip = this.ipsData.GetByValue(currentUserIpAddress) ?? new Ip { Value = currentUserIpAddress };

                contest.AllowedIps.Add(new ContestIp { Ip = ip, IsOriginallyAllowed = false });

                this.contestsData.Update(contest);
            }

            return this.RedirectToAction(GlobalConstants.Index, new { id = model.ContestId, official = true });
        }

        private bool IsUserAdminOrLecturerInContest(Contest contest) =>
            this.User.IsAdmin() ||
            contest.Lecturers.Any(c => c.LecturerId == this.UserProfile?.Id) ||
            contest.Category.Lecturers.Any(cl => cl.LecturerId == this.UserProfile?.Id);

        private Participant AddNewParticipantToContest(Contest contest, bool official)
        {
            if (contest.IsOnline &&
                official &&
                !this.IsUserAdminOrLecturerInContest(contest) &&
                !this.contestsData.IsUserInExamGroupByContestAndUser(contest.Id, this.UserProfile.Id))
            {
                throw new HttpException(
                    (int)HttpStatusCode.Forbidden,
                    Resource.ContestsGeneral.User_is_not_registered_for_exam);
            }

            var participant = this.participantsBusiness.CreateNewByContestByUserByIsOfficialAndIsAdmin(
                contest,
                this.UserProfile.Id,
                official,
                this.User.IsAdmin());

            return participant;
        }

        private void ValidateProblemForParticipant(
            Participant participant,
            Contest contest,
            int problemId,
            bool isOfficial)
        {
            if (isOfficial &&
                contest.IsOnline &&
                !this.IsUserAdminOrLecturerInContest(contest) &&
                participant.Problems.All(p => p.Id != problemId))
            {
                throw new HttpException(
                    (int)HttpStatusCode.Forbidden,
                    Resource.ContestsGeneral.Problem_not_assigned_to_user);
            }
        }
    }
}