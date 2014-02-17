namespace OJS.Web.Areas.Api.Models
{
    public class ErrorMessageViewModel
    {
        public ErrorMessageViewModel(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }
    }
}