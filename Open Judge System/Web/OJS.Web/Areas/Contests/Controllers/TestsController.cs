namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Common.Extensions;
    using OJS.Web.Controllers;
    using OJS.Web.Areas.Contests.ViewModels.Tests;

    using Resource = Resources.Areas.Contests.Controllers.Tests;

    public class TestsController : BaseController
    {
        public TestsController(IOjsData data)
            : base(data)
        {
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetInputData(int id)
        {
            var testInfo = this.Data.Tests
                .All()
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    InputDataAsBytes = t.InputData,
                    t.IsTrialTest,
                    t.ProblemId,
                    t.Problem.ContestId,
                    t.Problem.ShowDetailedFeedback
                })
                .FirstOrDefault();

            if (testInfo == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Test_not_found_message);
            }

            if (!testInfo.IsTrialTest &&
                !testInfo.ShowDetailedFeedback &&
                !this.CheckIfUserHasProblemPermissions(testInfo.ProblemId) &&
                !this.Data.Participants
                    .All()
                    .Any(p => p.UserId == this.UserProfile.Id && p.ContestId == testInfo.ContestId))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.No_rights_message);
            }

            var inputDataViewModel = new TestInputDataViewModel
            {
                InputData = testInfo.InputDataAsBytes.Decompress(),
                TestId = id
            };

            return this.PartialView("_GetInputDataPartial", inputDataViewModel);
        }
    }
}