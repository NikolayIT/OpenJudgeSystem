namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using EntityFramework.Extensions;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Services.Common.BackgroundJobs.Contracts;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Controllers;

    [AuthorizeRoles(SystemRole.Administrator)]
    public class TempController : AdministrationController
    {
        private readonly IBackgroundJobService backgroundJobs;

        public TempController(IOjsData data, IBackgroundJobService backgroundJobs)
            : base(data)
        {
            this.backgroundJobs = backgroundJobs;
        }

        public ActionResult RegisterJobForCleaningSubmissionsForProcessingTable()
        {
            string cron = "0 0 * * *";
            this.backgroundJobs.AddOrUpdateRecurringJob(
                "CleanSubmissionsForProcessingTable",
                () => this.CleanSubmissionsForProcessing(),
                cron);
            
            return null;
        }

        public ActionResult RegisterJobForDeletingLeftOverFilesInTempFolder()
        {
            var cron = "0 1 * * *";
            this.backgroundJobs.AddOrUpdateRecurringJob(
                "DeleteLeftOverFoldersInTempFolder",
                () => DirectoryHelpers.DeleteExecutionStrategyWorkingDirectories(),
                cron);

            return null;
        }

        private void CleanSubmissionsForProcessing()
        {
            this.Data.Context.SubmissionsForProcessing
                .Where(s => s.Processed && !s.Processing)
                .Delete();
        }
    }
}