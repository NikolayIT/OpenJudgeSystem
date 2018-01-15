namespace OJS.Services.Common.HttpRequester
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OJS.Services.Common.HttpRequester.Models;

    public class HttpRequesterService : IHttpRequesterService
    {
        public async Task<ExternalDataRetrievalResult<TData>> GetAsync<TData>(object requestData, string url, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                // TODO: Add to resource
                throw new ArgumentException("Невалиден URL.", nameof(url));
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // TODO: Add to resource
                throw new ArgumentException("Невалиден API ключ.", nameof(apiKey));
            }

            return await InternalGetAsync<TData>(requestData, url, apiKey);
        }

        private static async Task<ExternalDataRetrievalResult<TData>> InternalGetAsync<TData>(
            object requestData,
            string url,
            string apiKey)
        {
            var externalDataResult = new ExternalDataRetrievalResult<TData>();

            var queryStringSeparator = GetQueryStringSeparator(url);
            var requestUrl = $"{url}{queryStringSeparator}apiKey={apiKey}";

            using (var httpClient = new HttpClient())
            {
                // Using POST because of chance of enormous request data
                var response = await httpClient
                    .PostAsJsonAsync(requestUrl, requestData)
                    .ConfigureAwait(continueOnCapturedContext: false);

                if (response.IsSuccessStatusCode)
                {
                    using (var responseContentStream = await response.Content.ReadAsStreamAsync())
                    {
                        externalDataResult.Data = DeserializeJson<TData>(responseContentStream);
                    }
                }
                else
                {
                    // TODO: Add to resource
                    externalDataResult.ErrorMessage = await response.Content.ReadAsStringAsync()
                        ?? "Грешка при връзката с отдалечения сървър.";
                }
            }

            return externalDataResult;
        }

        private static string GetQueryStringSeparator(string url) =>
            url.IndexOf("?", StringComparison.Ordinal) >= 0 ? "&" : "?";

        private static T DeserializeJson<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var jsonSerializer = new JsonSerializer();
                    return jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }
}