namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers.Common;

    using DatabaseModelType = OJS.Data.Models.SubmissionType;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.SubmissionType.SubmissionTypeAdministrationViewModel;

    public class SubmissionTypesController : AdministrationBaseGridController
    {
        public SubmissionTypesController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.SubmissionTypes
                .All()
                .Select(ViewModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.SubmissionTypes
                .All()
                .FirstOrDefault(o => o.Id == (int)id);
        }

        public override string GetEntityKeyName()
        {
            return this.GetEntityKeyNameByType(typeof(DatabaseModelType));
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var databaseModel = model.GetEntityModel();
            model.Id = (int)this.BaseCreate(databaseModel);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.Id) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            return this.GridOperation(request, model);
        }

        [HttpPost]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            this.BaseDestroy(model.Id);
            return this.GridOperation(request, model);
        }
    }
}