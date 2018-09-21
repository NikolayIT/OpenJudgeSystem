namespace OJS.Services.Common
{
    public class ServiceResult
    {
        public static readonly ServiceResult Success = new ServiceResult(null);

        public ServiceResult(string error) => this.Error = error;

        public string Error { get; }

        public bool IsError => !string.IsNullOrWhiteSpace(this.Error);
    }
}