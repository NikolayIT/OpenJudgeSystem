namespace OJS.Web.Controllers
{
    using System;
    using System.Collections;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Expressions;
    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using NPOI.HSSF.UserModel;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Data;
    using OJS.Web.Areas.Administration.Controllers;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Infrastructure.Filters.Attributes;

    using AdminResource = Resources.Areas.Administration.AdministrationGeneral;

    [LogAccess]
    public abstract class AdministrationController : BaseController
    {
        protected AdministrationController(IOjsData data)
            : base(data)
        {
        }

        [NonAction]
        protected FileResult ExportToExcel([DataSourceRequest]DataSourceRequest request, IEnumerable data)
        {
            if (data == null)
            {
                throw new Exception("GetData() and DataType must be overridden");
            }

            // Get the data representing the current grid state - page, sort and filter
            request.PageSize = 0;
            IEnumerable items = data.ToDataSourceResult(request).Data;
            return this.CreateExcelFile(items);
        }

        [NonAction]
        protected FileResult CreateExcelFile(IEnumerable items)
        {
            Type dataType = items.GetType().GetGenericArguments()[0];

            var dataTypeProperties = dataType.GetProperties();

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
                            row.CreateCell(cellNumber).SetCellType(NPOI.SS.UserModel.CellType.Blank);
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
                                cell.SetCellType(NPOI.SS.UserModel.CellType.Numeric);
                            }
                            else if (typeCode == TypeCode.DateTime)
                            {
                                cell.SetCellValue((DateTime)propertyValue);
                            }
                            else
                            {
                                string propertyValueAsString = propertyValue.ToString();
                                if (propertyValue.ToString().Length > 10000)
                                {
                                    propertyValueAsString = "THIS CELL DOES NOT CONTAIN FULL INFORMATION: " + propertyValueAsString.Substring(0, 10000);
                                }

                                cell.SetCellValue(propertyValueAsString);
                            }
                        }

                        cellNumber++;
                    }
                }
            }

            sheet.AutoSizeColumns(columnNumber);

            // Write the workbook to a memory stream
            var outputStream = new MemoryStream();
            workbook.Write(outputStream);

            // Return the result to the end user
            return this.File(
                outputStream.ToArray(), // The binary data of the XLS file
                GlobalConstants.ExcelMimeType, // MIME type of Excel files
                string.Format("{0}.xls", this.GetType().Name)); // Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        protected ActionResult RedirectToContestsAdminPanelWithNoPrivilegesMessage()
        {
            this.TempData.AddDangerMessage(AdminResource.No_privileges_message);
            return this.RedirectToAction<ContestsController>(
                c => c.Index(),
                new { area = GlobalConstants.AdministrationAreaName });
        }
    }
}