using PortfolioTracker.Client.Interfaces;
using PortfolioTracker.Models;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PortfolioTracker.Client
{
    public class StockClient : IStockClient
    {
        private IRestClient _stockClient;

        public StockClient()
        {
            _stockClient = new RestClient("https://portfolio-tracker-api-dev.azurewebsites.net/api");
        }

        public async Task<ReturnObject<StockHistory>> GetHistory(string symbol)
        {
            var response = await _stockClient.ExecuteGetTaskAsync<ReturnObject<StockHistory>>(new RestRequest
            {
                Method = Method.GET,
                Resource = $"stocks/{symbol}/history"
            });

            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data;

            return new ReturnObject<StockHistory>
            {
                Success = false,
                Messages = new List<ReturnMessage>
                    {
                        new ReturnMessage
                        {
                            MessageType = MessageTypeEnum.Error,
                            Message = response.ErrorMessage
                        }
                    }
            };
        }
    }
}
