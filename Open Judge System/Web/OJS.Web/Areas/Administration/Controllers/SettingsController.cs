namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.Setting.SettingAdministrationViewModel;

    public class SettingsController : KendoGridAdministrationController
    {
        public SettingsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Settings
                .All()
                .Select(ModelType.ViewModel);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseCreate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseDestroy(request, model.ToEntity);
        }
    }
}