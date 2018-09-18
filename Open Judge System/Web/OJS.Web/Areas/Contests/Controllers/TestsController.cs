namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.Tests;
    using OJS.Web.Areas.Contests.ViewModels.Tests;
    using OJS.Web.Controllers;
    using OJS.Workers.Common.Extensions;

    using Resource = Resources.Areas.Contests.Controllers.Tests;

    [Authorize]
    public class TestsController : BaseController
    {
        private readonly ITestsDataService testsData;
        private readonly IParticipantsDataService participantsData;
        private readonly ISubmissionsDataService submissionsData;

        public TestsController(
            IOjsData data,
            ITestsDataService testsData,
            IParticipantsDataService participantsData,
            ISubmissionsDataService submissionsData)
            : base(data)
        {
            this.testsData = testsData;
            this.participantsData = participantsData;
            this.submissionsData = submissionsData;
        }

        [HttpGet]
        public ActionResult GetInputData(int testId, int submissionId)
        {
            var testInfo = this.testsData
                .GetByIdQuery(testId)
                .Select(t => new
                {
                    InputDataAsBytes = t.InputData,
                    t.IsTrialTest,
                    t.IsOpenTest,
                    t.ProblemId,
                    t.Problem.ProblemGroup.ContestId,
                    t.Problem.ShowDetailedFeedback,
                    t.Problem.ProblemGroup.Contest.AutoChangeTestsFeedbackVisibility
                })
                .FirstOrDefault();

            if (testInfo == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Test_not_found_message);
            }

            if (!ShouldDisplayDetailedTestInfo())
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.No_rights_message);
            }

            var inputDataViewModel = new TestInputDataViewModel
            {
                InputData = testInfo.InputDataAsBytes.Decompress(),
                TestId = testId
            };

            return this.PartialView("_GetInputDataPartial", inputDataViewModel);

            bool ShouldDisplayDetailedTestInfo()
            {
                var userHasProblemPermissions = this.CheckIfUserHasProblemPermissions(testInfo.ProblemId);

                var userIsParticipant = this.participantsData
                    .ExistsByContestAndUser(testInfo.ContestId, this.UserProfile.Id);

                var userHasTestPermissions = userIsParticipant &&
                    (testInfo.IsTrialTest || testInfo.ShowDetailedFeedback || testInfo.IsOpenTest);

                var isUnofficialSubmission = !this.submissionsData.IsOfficialById(submissionId);

                var testIsOpenForEveryone = testInfo.AutoChangeTestsFeedbackVisibility && isUnofficialSubmission;

                return userHasProblemPermissions || userHasTestPermissions || testIsOpenForEveryone;
            }
        }
    }
}