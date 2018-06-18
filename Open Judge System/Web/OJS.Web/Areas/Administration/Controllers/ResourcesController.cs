namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Net.Mime;
    using System.Web.Mvc;
    using System.Web.Mvc.Expressions;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Data.ProblemResources;
    using OJS.Services.Data.Problems;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.ProblemResource;
    using OJS.Web.Common;
    using OJS.Web.Common.Extensions;

    using GeneralResource = Resources.Areas.Administration.AdministrationGeneral;
    using Resource = Resources.Areas.Administration.Resources.ResourcesControllers;

    public class ResourcesController : LecturerBaseController
    {
        private readonly IProblemsDataService problemsData;
        private readonly IProblemResourcesDataService problemResourcesData;

        public ResourcesController(
            IOjsData data,
            IProblemsDataService problemsData,
            IProblemResourcesDataService problemResourcesData)
            : base(data)
        {
            this.problemsData = problemsData;
            this.problemResourcesData = problemResourcesData;
        }

        public JsonResult GetAll(int id, [DataSourceRequest] DataSourceRequest request)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Нямате привилегиите за това действие";
                return this.Json("No premmissions");
            }

            var resources = this.Data.Resources
                .All()
                .Where(res => res.ProblemId == id)
                .OrderBy(res => res.OrderBy)
                .Select(ProblemResourceGridViewModel.FromResource);

            return this.Json(resources.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Create(int id)
        {
            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var problem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Problem_not_found);
                return this.RedirectToAction(GlobalConstants.Index, "Problems");
            }

            int orderBy;
            var resources = problem.Resources.Where(res => !res.IsDeleted);

            if (!resources.Any())
            {
                orderBy = 0;
            }
            else
            {
                orderBy = resources.Max(res => res.OrderBy) + 1;
            }

            var resource = new ProblemResourceViewModel
            {
                ProblemId = id,
                ProblemName = problem.Name,
                OrderBy = orderBy,
                AllTypes = EnumConverter.GetSelectListItems<ProblemResourceType>()
            };

            return this.View(resource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, ProblemResourceViewModel resource)
        {
            if (resource == null)
            {
                this.TempData.AddDangerMessage(Resource.Invalid_resource);
                return this.RedirectToAction("Resource", "Problems", new { id });
            }

            if (!this.CheckIfUserHasProblemPermissions(id))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            if (resource.Type == ProblemResourceType.Link && string.IsNullOrEmpty(resource.RawLink))
            {
                this.ModelState.AddModelError("Link", Resource.Link_not_empty);
            }
            else if (resource.Type != ProblemResourceType.Link && (resource.File == null || resource.File.ContentLength == 0))
            {
                this.ModelState.AddModelError("File", Resource.File_required);
            }

            if (this.ModelState.IsValid)
            {
                var problem = this.Data.Problems
                    .All()
                    .FirstOrDefault(x => x.Id == id);

                if (problem == null)
                {
                    this.TempData.AddDangerMessage(Resource.Problem_not_found);
                    return this.RedirectToAction(GlobalConstants.Index, "Problems");
                }

                var newResource = new ProblemResource
                {
                    Name = resource.Name,
                    Type = resource.Type,
                    OrderBy = resource.OrderBy,
                };

                if (resource.Type == ProblemResourceType.Link)
                {
                    newResource.Link = resource.RawLink;
                }
                else
                {
                    newResource.File = resource.File.InputStream.ToByteArray();
                    newResource.FileExtension = resource.FileExtension;
                }

                problem.Resources.Add(newResource);
                this.Data.SaveChanges();

                return this.RedirectToAction("Resource", "Problems", new { id });
            }

            resource.AllTypes = EnumConverter.GetSelectListItems<ProblemResourceType>();
            return this.View(resource);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(Resource.Problem_not_found);
                return this.RedirectToAction(GlobalConstants.Index, "Problems");
            }

            var existingResource = this.Data.Resources.All()
                .Where(res => res.Id == id)
                .Select(ProblemResourceViewModel.FromProblemResource)
                .FirstOrDefault();

            if (existingResource == null)
            {
                this.TempData.AddDangerMessage(Resource.Problem_not_found);
                return this.RedirectToAction(GlobalConstants.Index, "Problems");
            }

            if (!this.CheckIfUserHasProblemPermissions(existingResource.ProblemId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            existingResource.AllTypes = EnumConverter.GetSelectListItems<ProblemResourceType>();

            return this.View(existingResource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, ProblemResourceViewModel resource)
        {
            if (id == null)
            {
                this.TempData.AddDangerMessage(Resource.Problem_not_found);
                return this.RedirectToAction(GlobalConstants.Index, "Problems");
            }

            if (this.ModelState.IsValid)
            {
                var existingResource = this.Data.Resources
                    .All()
                    .FirstOrDefault(res => res.Id == id);

                if (existingResource == null)
                {
                    this.TempData.AddDangerMessage(Resource.Resource_not_found);
                    return this.RedirectToAction(GlobalConstants.Index, "Problems");
                }

                if (!this.CheckIfUserHasProblemPermissions(existingResource.ProblemId))
                {
                    return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
                }

                existingResource.Name = resource.Name;
                existingResource.Type = resource.Type;
                existingResource.OrderBy = resource.OrderBy;

                if (existingResource.Type == ProblemResourceType.Link && !string.IsNullOrEmpty(resource.RawLink))
                {
                    existingResource.Link = resource.RawLink;
                }
                else if (resource.Type != ProblemResourceType.Link && resource.File != null && resource.File.ContentLength > 0)
                {
                    existingResource.File = resource.File.InputStream.ToByteArray();
                    existingResource.FileExtension = resource.FileExtension;
                }

                this.Data.SaveChanges();

                return this.RedirectToAction("Resource", "Problems", new { id = existingResource.ProblemId });
            }

            resource.AllTypes = EnumConverter.GetSelectListItems<ProblemResourceType>();
            return this.View(resource);
        }

        [HttpGet]
        public JsonResult Delete(ProblemResourceGridViewModel resource, [DataSourceRequest] DataSourceRequest request)
        {
            this.problemResourcesData.DeleteById(resource.Id);

            return this.Json(new[] { resource }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Download(int id)
        {
            var resource = this.problemResourcesData.GetById(id);

            if (resource == null)
            {
                this.TempData.AddDangerMessage(Resource.Resource_not_found);
                return this.RedirectToAction<ProblemsController>(
                    c => c.Index(),
                    new { area = GlobalConstants.AdministrationAreaName });
            }

            var problem = this.problemsData
                .GetByIdQuery(resource.ProblemId)
                .Select(p => new
                {
                    p.Id,
                    p.Name
                })
                .FirstOrDefault(pr => pr.Id == resource.ProblemId);

            if (problem == null)
            {
                this.TempData.AddDangerMessage(Resource.Problem_not_found);
                return this.RedirectToAction<ProblemsController>(
                    c => c.Index(),
                    new { area = GlobalConstants.AdministrationAreaName });
            }

            if (!this.CheckIfUserHasProblemPermissions(problem.Id))
            {
                this.TempData.AddDangerMessage(GeneralResource.No_privileges_message);
                return this.RedirectToAction<ContestsController>(
                    c => c.Index(),
                    new { area = GlobalConstants.AdministrationAreaName });
            }

            var fileResult = resource.File.ToStream();

            var fileName = "Resource-" + resource.Id + "-" + problem.Name.Replace(" ", string.Empty) + "." + resource.FileExtension;

            return this.File(fileResult, MediaTypeNames.Application.Octet, fileName);
        }
    }
}