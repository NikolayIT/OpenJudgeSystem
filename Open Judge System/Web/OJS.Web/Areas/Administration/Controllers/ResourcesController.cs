namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Linq;
    using System.Net.Mime;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.ProblemResource;
    using OJS.Web.Common;
    using OJS.Web.Controllers;

    public class ResourcesController : AdministrationController
    {
        public ResourcesController(IOjsData data)
            : base(data)
        {
        }

        public JsonResult GetAll(int id, [DataSourceRequest] DataSourceRequest request)
        {
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
            var problem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Задачата не е намерена";
                return this.RedirectToAction(GlobalConstants.Index, "Problems");
            }

            int orderBy;
            var resources = problem.Resources.Where(res => !res.IsDeleted);

            if (resources == null || resources.Count() == 0)
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
                this.TempData[GlobalConstants.DangerMessage] = "Ресурсът е невалиден";
                return this.RedirectToAction("Resource", "Problems", new { id = id });
            }

            if (resource.Type == ProblemResourceType.Video && string.IsNullOrEmpty(resource.Link))
            {
                this.ModelState.AddModelError("Link", "Линкът не може да бъде празен");
            }
            else if (resource.Type != ProblemResourceType.Video && (resource.File == null || resource.File.ContentLength == 0))
            {
                this.ModelState.AddModelError("File", "Файлът е задължителен");
            }

            if (this.ModelState.IsValid)
            {
                var problem = this.Data.Problems
                    .All()
                    .FirstOrDefault(x => x.Id == id);

                if (problem == null)
                {
                    this.TempData[GlobalConstants.DangerMessage] = "Задачата не е намерена";
                    return this.RedirectToAction(GlobalConstants.Index, "Problems");
                }

                var newResource = new ProblemResource
                {
                    Name = resource.Name,
                    Type = resource.Type,
                    OrderBy = resource.OrderBy,
                };

                if (resource.Type == ProblemResourceType.Video)
                {
                    newResource.Link = resource.Link;
                }
                else
                {
                    newResource.File = resource.File.InputStream.ToByteArray();
                    newResource.FileExtension = resource.FileExtension;
                }

                problem.Resources.Add(newResource);
                this.Data.SaveChanges();

                return this.RedirectToAction("Resource", "Problems", new { id = id });
            }

            resource.AllTypes = EnumConverter.GetSelectListItems<ProblemResourceType>();
            return this.View(resource);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Задачата не е намерена";
                return this.RedirectToAction(GlobalConstants.Index, "Problems");
            }

            var existingResource = this.Data.Resources.All()
                .Where(res => res.Id == id)
                .Select(ProblemResourceViewModel.FromProblemResource)
                .FirstOrDefault();

            if (existingResource == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Задачата не е намерена";
                return this.RedirectToAction(GlobalConstants.Index, "Problems");
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
                this.TempData[GlobalConstants.DangerMessage] = "Задачата не е намерена";
                return this.RedirectToAction(GlobalConstants.Index, "Problems");
            }

            if (this.ModelState.IsValid)
            {
                var existingResource = this.Data.Resources
                    .All()
                    .FirstOrDefault(res => res.Id == id);

                if (existingResource == null)
                {
                    this.TempData[GlobalConstants.DangerMessage] = "Ресурсът не е намерен";
                    return this.RedirectToAction(GlobalConstants.Index, "Problems");
                }

                existingResource.Name = resource.Name;
                existingResource.Type = resource.Type;
                existingResource.OrderBy = resource.OrderBy;

                if (existingResource.Type == ProblemResourceType.Video && !string.IsNullOrEmpty(resource.Link))
                {
                    existingResource.Link = resource.Link;
                }
                else if (resource.Type != ProblemResourceType.Video && resource.File != null && resource.File.ContentLength > 0)
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
            this.Data.Resources.Delete(resource.Id);
            this.Data.SaveChanges();

            return this.Json(new[] { resource }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Download(int id)
        {
            var resource = this.Data.Resources
                .All()
                .FirstOrDefault(res => res.Id == id);

            var problem = this.Data.Problems
                .All()
                .FirstOrDefault(pr => pr.Id == resource.ProblemId);

            if (resource == null)
            {
                this.TempData[GlobalConstants.DangerMessage] = "Ресурса не е намерен";
                return this.Redirect("/Administration/Problems/Contest/" + resource.Problem.ContestId);
            }

            var fileResult = resource.File.ToStream();
            var fileName = "Resource-" + resource.Id + "-" + problem.Name.Replace(" ", string.Empty) + "." + resource.FileExtension;

            return this.File(fileResult, MediaTypeNames.Application.Octet, fileName);
        }
    }
}