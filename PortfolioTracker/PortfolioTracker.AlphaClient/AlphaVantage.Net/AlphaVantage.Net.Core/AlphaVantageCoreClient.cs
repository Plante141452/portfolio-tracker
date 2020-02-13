using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Core.Exceptions;
using PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Core.Validation;

namespace PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Core
{
    public class AlphaVantageCoreClient
    {
        [CanBeNull]
        private readonly IApiCallValidator _apiCallValidator;

        private static readonly HttpClient Client = new HttpClient();

        public AlphaVantageCoreClient(IApiCallValidator apiCallValidator = null)
        {
            _apiCallValidator = apiCallValidator;
        }

        public virtual async Task<JObject> RequestApiAsync(string apiKey, ApiFunction function, IDictionary<string, string> query = null)
        {
            AssertValid(function, query);

            var request = ComposeHttpRequest(apiKey, function, query);
            var response = await Client.SendAsync(request);

            var jsonString = await response.Content.ReadAsStringAsync();
            var jObject = (JObject)JsonConvert.DeserializeObject(jsonString);

            AssertNotBadRequest(jObject);

            return jObject;
        }

        private HttpRequestMessage ComposeHttpRequest(string apiKey, ApiFunction function, IDictionary<string, string> query)
        {
            var fullQueryDict = new Dictionary<string, string>(query);
            fullQueryDict.Add(ApiConstants.ApiKeyQueryVar, apiKey);
            fullQueryDict.Add(ApiConstants.FunctionQueryVar, function.ToString());

            var urlWithQueryString = QueryHelpers.AddQueryString(ApiConstants.AlfaVantageUrl, fullQueryDict);
            var urlWithQuery = new Uri(urlWithQueryString);

            var request = new HttpRequestMessage
            {
                RequestUri = urlWithQuery,
                Method = HttpMethod.Get
            };

            return request;
        }

        private void AssertValid(ApiFunction function, IDictionary<string, string> query = null)
        {
            if (_apiCallValidator == null) return;

            var validationResult = _apiCallValidator.Validate(function, query);

            if (!validationResult.IsValid)
                throw new AlphaVantageException(validationResult.ErrorMsg);
        }

        private void AssertNotBadRequest(JObject jObject)
        {
            if (jObject.ContainsKey(ApiConstants.BadRequestToken))
                throw new AlphaVantageException(jObject[ApiConstants.BadRequestToken].ToString());
        }
    }
}