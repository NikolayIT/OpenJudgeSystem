namespace OJS.Web.Common.Models
{
    using System.Collections.Generic;

    using OJS.Common.Extensions;

    public class JsonResponse
    {
        public bool Success => this.ErrorMessages.IsNullOrEmpty();

        public object OriginalData { get; set; }

        public IEnumerable<string> ErrorMessages { get; set; }
    }
}