namespace OJS.Web.Common.Models
{
    public class JsonResponse
    {
        public bool Success => this.ErrorMessage == null;

        public object Data { get; set; }

        public string SuccessMessage { get; set; }

        public string ErrorMessage { get; set; }
    }
}