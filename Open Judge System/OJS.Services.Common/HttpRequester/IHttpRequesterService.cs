namespace OJS.Services.Common.HttpRequester
{
    using System.Threading.Tasks;

    using OJS.Services.Common.HttpRequester.Models;

    public interface IHttpRequesterService : IService
    {
        ExternalDataRetrievalResult<TData> Get<TData>(
            object requestData,
            string url,
            string apiKey);

        Task<ExternalDataRetrievalResult<TData>> GetAsync<TData>(
            object requestData,
            string url,
            string apiKey);
    }
}