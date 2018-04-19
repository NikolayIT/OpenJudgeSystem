namespace OJS.Web.Common.Extensions
{
    using NPOI.SS.UserModel;

    public static class SheetExtensions
    {
        public static void AutoSizeColumns(this ISheet sheet, int columnsCount)
        {
            for (var i = 0; i < columnsCount; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }
    }
}