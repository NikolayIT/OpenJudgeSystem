namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.User.UserAdministrationViewModel;

    public class UsersController : KendoGridAdministrationController
    {
        public UsersController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Users
                .All()
                .Select(ModelType.ViewModel);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            return this.BaseUpdate(request, model.ToEntity);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ModelType model)
        {
            this.Data.Users.Delete(model.ToEntity);

            return this.Json(ModelState.ToDataSourceResult());
        }
    }
}