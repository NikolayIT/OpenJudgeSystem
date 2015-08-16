namespace OJS.Web.Common.Extensions
{
    using System.Web.Mvc;

    using OJS.Common;

    public static class TempDataExtentions
    {
        public static void AddDangerMessage(this TempDataDictionary tempData, string message)
        {
            tempData.Add(GlobalConstants.DangerMessage, message);
        }

        public static void AddInfoMessage(this TempDataDictionary tempData, string message)
        {
            tempData.Add(GlobalConstants.InfoMessage, message);
        }
    }
}
