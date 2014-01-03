namespace OJS.Web.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

    using NPOI.HSSF.UserModel;

    using OJS.Common.DataAnnotations;
    using OJS.Data;
    using OJS.Web.Common.Interfaces;

    public abstract class KendoGridAdministrationController : AdministrationController, IKendoGridAdministrationController
    {
        protected KendoGridAdministrationController(IOjsData data)
            : base(data)
        {
        }

        public abstract IEnumerable GetData();

        public abstract object GetById(object id);

        [HttpPost]
        public virtual ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            var data = this.GetData();
            var serializationSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var json = JsonConvert.SerializeObject(data.ToDataSourceResult(request), Formatting.None, serializationSettings);
            return this.Content(json, "application/json");
        }

        [HttpGet]
        public FileResult ExportToExcel([DataSourceRequest] DataSourceRequest request)
        {
            return this.ExportToExcel(request, this.GetData());
        }

        [NonAction]
        protected void BaseCreate(object model)
        {
            if (model != null && ModelState.IsValid)
            {
                var itemForAdding = this.Data.Context.Entry(model);
                itemForAdding.State = EntityState.Added;
                this.Data.SaveChanges();
            }
        }

        [NonAction]
        protected void BaseUpdate(object model)
        {
            if (model != null && ModelState.IsValid)
            {
                var itemForUpdating = this.Data.Context.Entry(model);
                itemForUpdating.State = EntityState.Modified;
                this.Data.SaveChanges();
            }
        }

        [NonAction]
        protected void BaseDestroy(object id)
        {
            var model = this.GetById(id);
            if (model != null)
            {
                var itemForDeletion = this.Data.Context.Entry(model);
                if (itemForDeletion != null)
                {
                    itemForDeletion.State = EntityState.Deleted;
                    this.Data.SaveChanges();
                }
            }
        }

        [NonAction]
        protected JsonResult GridOperation([DataSourceRequest]DataSourceRequest request, object model)
        {
            return this.Json(new[] { model }.ToDataSourceResult(request, this.ModelState));
        }

        [NonAction]
        private FileResult ExportToExcel([DataSourceRequest]DataSourceRequest request, IEnumerable data)
        {
            Type dataType = data.GetType().GetGenericArguments()[0];
            var dataTypeProperties = dataType.GetProperties();
            if (data == null || dataType == null)
            {
                throw new Exception("GetData() and DataType must be overriden");
            }

            // Get the data representing the current grid state - page, sort and filter
            request.PageSize = 0;
            IEnumerable items = data.ToDataSourceResult(request).Data;

            // Create new Excel workbook
            var workbook = new HSSFWorkbook();

            // Create new Excel sheet
            var sheet = workbook.CreateSheet();

            // Create a header row
            var headerRow = sheet.CreateRow(0);
            int columnNumber = 0;
            foreach (var property in dataTypeProperties)
            {
                bool include = true;
                object[] excludeAttributes = property.GetCustomAttributes(typeof(ExcludeFromExcelAttribute), true);
                if (excludeAttributes.Any())
                {
                    include = false;
                }

                if (include)
                {
                    string cellName = property.Name;
                    object[] attributes = property.GetCustomAttributes(typeof(DisplayAttribute), true);
                    if (attributes.Any())
                    {
                        var attribute = attributes[0] as DisplayAttribute;
                        if (attribute != null)
                        {
                            cellName = attribute.Name ?? property.Name;
                        }
                    }

                    headerRow.CreateCell(columnNumber++).SetCellValue(cellName);
                }
            }

            // (Optional) freeze the header row so it is not scrolled
            sheet.CreateFreezePane(0, 1, 0, 1);

            int rowNumber = 1;

            // Populate the sheet with values from the grid data
            foreach (object item in items)
            {
                // Create a new row
                var row = sheet.CreateRow(rowNumber++);

                int cellNumber = 0;
                foreach (var property in dataTypeProperties)
                {
                    bool include = true;
                    object[] excludeAttributes = property.GetCustomAttributes(typeof(ExcludeFromExcelAttribute), true);
                    if (excludeAttributes.Any())
                    {
                        include = false;
                    }

                    if (include)
                    {
                        object propertyValue = item.GetType().GetProperty(property.Name).GetValue(item, null);
                        if (propertyValue == null)
                        {
                            row.CreateCell(cellNumber).SetCellType(NPOI.SS.UserModel.CellType.BLANK);
                        }
                        else
                        {
                            var cell = row.CreateCell(cellNumber);
                            double value;
                            var typeCode = Type.GetTypeCode(property.PropertyType);
                            if (typeCode == TypeCode.Single || typeCode == TypeCode.Char)
                            {
                                cell.SetCellValue(propertyValue.ToString());
                            }

                            if (double.TryParse(propertyValue.ToString(), out value))
                            {
                                cell.SetCellValue(value);
                                cell.SetCellType(NPOI.SS.UserModel.CellType.NUMERIC);
                            }
                            else if (typeCode == TypeCode.DateTime)
                            {
                                cell.SetCellValue((DateTime)propertyValue);
                            }
                            else
                            {
                                cell.SetCellValue(propertyValue.ToString());
                            }
                        }

                        cellNumber++;
                    }
                }
            }

            // Auto-size all columns
            for (int i = 0; i < columnNumber; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            // Write the workbook to a memory stream
            var outputStream = new MemoryStream();
            workbook.Write(outputStream);

            // Return the result to the end user
            return this.File(
                outputStream.ToArray(), // The binary data of the XLS file
                "application/vnd.ms-excel", // MIME type of Excel files
                string.Format("{0}.xls", this.GetType().Name)); // Suggested file name in the "Save as" dialog which will be displayed to the end user
        }
    }
}