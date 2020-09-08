using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortfolioTracker.IexClient
{
    public class IexClient : IIexClient
    {
        private readonly RestClient _client;

        public IexClient()
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
                    new Parameter("token", "sk_8f810bc028dc49eb91276f71ccbf36e9", ParameterType.QueryString)
                }
            };

            return await _client.GetAsync<T>(request);
        }

        private DateTimeOffset ToClosestFriday(DateTimeOffset date, bool allowFuture = false)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Thursday:
                    return date.AddDays(allowFuture ? 1 : -6);
                case DayOfWeek.Wednesday:
                    return date.AddDays(allowFuture ? 2 : -5);
                case DayOfWeek.Tuesday:
                    return date.AddDays(allowFuture ? 3 : -4);
                case DayOfWeek.Monday:
                    return date.AddDays(-3);
                case DayOfWeek.Sunday:
                    return date.AddDays(-2);
                case DayOfWeek.Saturday:
                    return date.AddDays(-1);
            }

            return date;
        }

        public async Task<List<IexHistoryContract>> GetHistory(string symbol, TimeCadenceEnum cadence)
        {
            var friday = ToClosestFriday(DateTimeOffset.Now.Date);

            var history = await Get<List<IexHistoryContract>>($"stock/{symbol}/chart/5y?chartCloseOnly=true");

            var index = history.FindLastIndex(x => x.date.Date <= friday);
            var fridays = new List<IexHistoryContract>();

            while (index >= 0)
            {
                fridays.Add(history[index]);

                switch (cadence)
                {
                    case TimeCadenceEnum.Daily:
                        friday = friday.AddDays(-1);
                        break;
                    case TimeCadenceEnum.Weekly:
                        friday = friday.AddDays(-7);
                        break;
                    case TimeCadenceEnum.Monthly:
                        friday = ToClosestFriday(friday.AddMonths(-1), true);
                        break;
                    case TimeCadenceEnum.Quarterly:
                        friday = ToClosestFriday(friday.AddMonths(-3), true);
                        break;
                    case TimeCadenceEnum.Yearly:
                        friday = ToClosestFriday(friday.AddYears(-1), true);
                        break;
                }

                index = history.FindLastIndex(x => x.date.Date <= friday);
            }

            return fridays;
        }

        public async Task<IexQuoteContract> GetQuote(string symbol)
        {
            return await Get<IexQuoteContract>($"stock/{symbol}/quote");
        }
    }

    public class IexHistoryContract
    {
        public decimal change { get; set; }
        public decimal changeOverTime { get; set; }
        public decimal changePercent { get; set; }
        public decimal close { get; set; }
        public DateTimeOffset date { get; set; }
        public decimal high { get; set; }
        public string label { get; set; }
        public decimal low { get; set; }
        public decimal open { get; set; }
        public decimal uClose { get; set; }
        public decimal uHigh { get; set; }
        public decimal uLow { get; set; }
        public decimal uOpen { get; set; }
        public int uVolume { get; set; }
        public int volume { get; set; }
    }

    public class IexQuoteContract
    {
        public string symbol { get; set; }
        public double change { get; set; }
        public double latestPrice { get; set; }
        public DateTimeOffset latestUpdate { get; set; }
    }

    public enum TimeCadenceEnum
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly
    }
}