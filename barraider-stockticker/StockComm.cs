﻿using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BarRaider.StockTicker
{
    internal class StockComm
    {
        private const string STOCK_URI_PREFIX = "https://api.iextrading.com/1.0/stock/";
        private const string STOCK_BATCH_CHART_QUOTE = "market/batch";
        private const string CHART_DAILY = "1m";
        private const string CHART_MINUTE = "1d";
        private const int DEFAULT_CHART_POINTS = 36;

        public async Task<SymbolData> GetSymbol(string stockSymbol)
        {
            var kvp = new List<KeyValuePair<string, string>>();
            kvp.Add(new KeyValuePair<string, string>("symbols", stockSymbol));
            kvp.Add(new KeyValuePair<string, string>("types", "quote")); // Remove chart as of now
            //kvp.Add(new KeyValuePair<string, string>("types", "quote,chart"));
            kvp.Add(new KeyValuePair<string, string>("range", "dynamic"));
            //kvp.Add(new KeyValuePair<string, string>("chartLast", DEFAULT_CHART_POINTS.ToString()));
            HttpResponseMessage response = await StockQuery(STOCK_BATCH_CHART_QUOTE, kvp);

            if (response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(body);

                // Invalid Stock Symbol
                if (obj.Count == 0)
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"GetSymbol invalid symbol: {stockSymbol}");
                    return null;
                }

                var jp = obj.Properties().First();
                StockQuote quote = jp.Value["quote"].ToObject<StockQuote>();

                ChartBase[] chart = null;
                
                /* Not used at this point
                if (jp.Value["chart"]["range"].ToString() == CHART_DAILY)
                {
                    chart = GetDailyChart(jp.Value["chart"]["data"]);
                }
                else
                {
                    chart = GetMinuteChart(jp.Value["chart"]["data"]);
                }
                */
                return new SymbolData(quote.Symbol, quote, chart);
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"GetSymbol invalid response: {response.StatusCode}");
            }
            return null;
        }

        private DailyChart[] GetDailyChart(JToken data)
        {
            return data.ToObject<DailyChart[]>();
        }

        private MinuteChart[] GetMinuteChart(JToken data)
        {
            return data.ToObject<MinuteChart[]>();
        }

        private async Task<HttpResponseMessage> StockQuery(string uriPath, List<KeyValuePair<string, string>> optionalContent)
        {
            string queryParams = string.Empty;
            var client = new HttpClient();
            //client.Timeout = new TimeSpan(0, 0, 10);

            if (optionalContent != null)
            {
                var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
                foreach (var kvp in optionalContent)
                {
                    query[kvp.Key] = kvp.Value;

                }
                queryParams = "?" + query.ToString();
            }
            return await client.GetAsync($"{STOCK_URI_PREFIX}{uriPath}{queryParams}");
        }
    }
}
