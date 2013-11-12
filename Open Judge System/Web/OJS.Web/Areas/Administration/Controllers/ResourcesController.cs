namespace OJS.Web.Areas.Administration.Controllers
{
    using Kendo.Mvc.UI;
    using Kendo.Mvc.Extensions;
    using System;
    using System.Linq;
    using System.Net.Mime;
    using System.Web.Mvc;
    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Web.Controllers;
    using OJS.Web.Areas.Administration.ViewModels;
    using OJS.Data.Models;
    using System.Collections.Generic;

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

            return Json(resources.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Create(int id)
        {
            var problem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == id);

            if (problem == null)
            {
                TempData["DangerMessage"] = "Задачата не е намерена";
                return RedirectToAction("Index", "Problems");
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
                AllTypes = this.GetAllResourceTypes()
            };

            return View(resource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, ProblemResourceViewModel resource)
        {
            if (resource == null)
            {
                TempData["DangerMessage"] = "Ресурсът е невалиден";
                return RedirectToAction("Resource", "Problems", new { id = id });
            }

            if (resource.Type == ProblemResourceType.Video && string.IsNullOrEmpty(resource.Link))
            {
                ModelState.AddModelError("Link", "Линкът не може да бъде празен");
            }
            else if (resource.Type != ProblemResourceType.Video && (resource.File == null || resource.File.ContentLength == 0))
            {
                ModelState.AddModelError("File", "Файлът е задължителен");
            }

            if (ModelState.IsValid)
            {
                var problem = this.Data.Problems
                    .All()
                    .FirstOrDefault(x => x.Id == id);

                if (problem == null)
                {
                    TempData["DangerMessage"] = "Задачата не е намерена";
                    return RedirectToAction("Index", "Problems");
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

                return RedirectToAction("Resource", "Problems", new { id = id });
            }

            resource.AllTypes = this.GetAllResourceTypes();
            return View(resource);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["DangerMessage"] = "Задачата не е намерена";
                return RedirectToAction("Index", "Problems");
            }

            var existingResource = this.Data.Resources.All()
                .Where(res => res.Id == id)
                .Select(ProblemResourceViewModel.FromProblemResource)
                .FirstOrDefault();

            if (existingResource == null)
            {
                TempData["DangerMessage"] = "Задачата не е намерена";
                return RedirectToAction("Index", "Problems");
            }

            existingResource.AllTypes = this.GetAllResourceTypes();

            return View(existingResource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, ProblemResourceViewModel resource)
        {
            if (id == null)
            {
                TempData["DangerMessage"] = "Задачата не е намерена";
                return RedirectToAction("Index", "Problems");
            }

            if (ModelState.IsValid)
            {
                var existingResource = this.Data.Resources
                    .All()
                    .FirstOrDefault(res => res.Id == id);

                if (existingResource == null)
                {
                    TempData["DangerMessage"] = "Ресурсът не е намерен";
                    return RedirectToAction("Index", "Problems");
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

                return RedirectToAction("Resource", "Problems", new { id = existingResource.ProblemId });
            }

            resource.AllTypes = this.GetAllResourceTypes();
            return View(resource);
        }

        public JsonResult Delete(ProblemResourceGridViewModel resource, [DataSourceRequest] DataSourceRequest request)
        {
            this.Data.Resources.Delete(resource.Id);
            this.Data.SaveChanges();

            return Json(new[] { resource }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
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
                TempData["DangerMessage"] = "Ресурса не е намерен";
                return Redirect("/Administration/Problems/Contest/" + resource.Problem.ContestId);
            }

            var fileResult = resource.File.ToStream();
            var fileName = "Resource-" + resource.Id + "-" + problem.Name.Replace(" ", string.Empty) + "." + resource.FileExtension;

            return File(fileResult, MediaTypeNames.Application.Octet, fileName);
        }

        private IEnumerable<SelectListItem> GetAllResourceTypes()
        {
            var allTypes = Enum.GetValues(typeof(ProblemResourceType)).Cast<ProblemResourceType>().Select(v => new SelectListItem
            {
                Text = v.GetDescription(),
                Value = ((int)v).ToString()
            });

            return allTypes;
        }
    }
}