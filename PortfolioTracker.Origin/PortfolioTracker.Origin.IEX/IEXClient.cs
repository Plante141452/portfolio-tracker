using System;
using System.Threading.Tasks;
using RestSharp;

namespace PortfolioTracker.Origin.IEX
{
    public class IEXQuote
    {
        public string symbol { get; set; }
        public decimal change { get; set; }
        public decimal latestPrice { get; set; }
        public DateTimeOffset latestUpdate { get; set; }
    }

    public class IEXClient : IIEXClient
    {
        private RestClient _client;

        public IEXClient()
        {
            _client = new RestClient("https://cloud.iexapis.com/stable");
        }

        private async Task<T> Get<T>(string uri) where T : new()
        {
            var request = new RestRequest
            {
                Resource = uri,
                Method = Method.GET,
                Parameters =
                {
                    new Parameter
                    {
                        Name = "token",
                        Value = "sk_8f810bc028dc49eb91276f71ccbf36e9",
                        Type = ParameterType.QueryString
                    }
                }
            };
            var response = await _client.ExecuteGetTaskAsync<T>(request);
            return response.Data;
        }

        public async Task<IEXQuote> GetQuote(string symbol)
        {
            return await Get<IEXQuote>($"stock/{symbol}/quote");
        }
    }
}