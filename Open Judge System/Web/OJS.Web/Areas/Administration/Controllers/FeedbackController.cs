namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Administration.ViewModels;
    using OJS.Web.Controllers;

    using ModelType = OJS.Data.Models.FeedbackReport;

    public class FeedbackController : KendoGridAdministrationController
    {
        public FeedbackController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.FeedbackReports.All().Select(FeedbackReportViewModel.FromFeedbackReport);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseCreate(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseDestroy(request, model);
        }
    }
}