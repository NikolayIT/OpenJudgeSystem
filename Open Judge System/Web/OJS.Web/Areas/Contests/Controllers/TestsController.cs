namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Web.Areas.Contests.ViewModels.Tests;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests.Controllers.Tests;

    [Authorize]
    public class TestsController : BaseController
    {
        public TestsController(IOjsData data)
            : base(data)
        {
        }

        [HttpGet]
        public ActionResult GetInputData(int testId, int submissionId)
        {
            var testInfo = this.Data.Tests
                .All()
                .Where(t => t.Id == testId)
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

            var hasPermissions = this.CheckIfUserHasProblemPermissions(testInfo.ProblemId);
            var isParticipant = this.Data.Participants
                .All()
                .Any(p => p.UserId == this.UserProfile.Id && p.ContestId == testInfo.ContestId);

            var isUnofficialParticipant = this.Data.Submissions
                .All()
                .Any(s => s.Id == submissionId && !s.Participant.IsOfficial);

            bool shouldDisplayDetailedTestInfo = hasPermissions ||
                                                 ((testInfo.IsTrialTest ||
                                                   testInfo.ShowDetailedFeedback ||
                                                   testInfo.IsOpenTest) &&
                                                   isParticipant) ||
                                                 (testInfo.AutoChangeTestsFeedbackVisibility && isUnofficialParticipant);

            if (shouldDisplayDetailedTestInfo)
            {
                var inputDataViewModel = new TestInputDataViewModel
                {
                    InputData = testInfo.InputDataAsBytes.Decompress(),
                    TestId = testId
                };

                return this.PartialView("_GetInputDataPartial", inputDataViewModel);
            }

            throw new HttpException((int)HttpStatusCode.Forbidden, Resource.No_rights_message);
        }
    }
}