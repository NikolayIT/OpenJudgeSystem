namespace OJS.Services.Common.HttpRequester.Models
{
    public class ExternalDataRetrievalResult<TData>
    {
        public ExternalDataRetrievalResult()
            : this(default(TData), null)
        {
        }

        public ExternalDataRetrievalResult(TData data, string errorMessage)
        {
            this.Data = data;
            this.ErrorMessage = errorMessage;
        }

        public TData Data { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsSuccess => string.IsNullOrWhiteSpace(this.ErrorMessage);
    }
}