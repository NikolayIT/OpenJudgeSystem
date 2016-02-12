using OJS.Common.Extensions;
using OJS.Web.Areas.Contests.ViewModels.Tests;

namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Controllers;
    using OJS.Web.ViewModels.TestRun;

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
            var testInputData = this.Data.Tests
                .All()
                .Where(t => t.Id == id)
                .Select(t => new
                    {
                        TestId = t.Id,
                        InputDataAsBytes = t.InputData,
                        t.IsTrialTest,
                        t.ProblemId,
                        t.Problem.ContestId,
                        t.Problem.ShowDetailedFeedback
                    })
                .FirstOrDefault();

            if (testInputData == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Test_not_found_message);
            }

            if (!testInputData.IsTrialTest &&
                !testInputData.ShowDetailedFeedback &&
                !this.CheckIfUserHasProblemPermissions(testInputData.ProblemId))
            {
                var userIsParticipant = this.Data.Participants
                    .All()
                    .Any(p => p.UserId == this.UserProfile.Id && p.ContestId == testInputData.ContestId);

                if (!userIsParticipant)
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, Resource.No_rights_message);
                }
            }

            var inputDataViewModel = new TestInputDataViewModel()
            {
                InputData = testInputData.InputDataAsBytes.Decompress(),
                TestId = testInputData.TestId
            };

            return this.PartialView("_GetInputDataPartial", inputDataViewModel);
        }
    }
}