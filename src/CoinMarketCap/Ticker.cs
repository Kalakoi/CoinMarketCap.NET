using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kalakoi.Crypto.CoinMarketCap
{
    /// <summary>
    /// Provides access to ticker data provided by CoinMarketCap
    /// </summary>
    public class Ticker
    {
        /// <summary>
        /// ID specific to CoinMarketCap.
        /// </summary>
        public string ID { get; private set; }
        /// <summary>
        /// Currency name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Ticker symbol.
        /// </summary>
        public string Symbol { get; private set; }
        /// <summary>
        /// Rank among all active assets by market capitalization.
        /// </summary>
        public int Rank { get; private set; }
        /// <summary>
        /// Price per unit in USD.
        /// </summary>
        public double PriceUSD { get; private set; }
        /// <summary>
        /// Price per unit in BTC.
        /// </summary>
        public double PriceBTC { get; private set; }
        /// <summary>
        /// 24 hour trade volume in USD.
        /// </summary>
        public double Volume24HourUSD { get; private set; }
        /// <summary>
        /// Market capitalization in USD.
        /// </summary>
        public double MarketCapUSD { get; private set; }
        /// <summary>
        /// Available coin supply.
        /// </summary>
        public double AvailableSupply { get; private set; }
        /// <summary>
        /// Total coin supply.
        /// </summary>
        public double TotalSupply { get; private set; }
        /// <summary>
        /// Maximum coin supply, if applicable.
        /// </summary>
        public double MaxSupply { get; private set; }
        /// <summary>
        /// Percentage price change in last hour.
        /// </summary>
        public double PercentChange1Hour { get; private set; }
        /// <summary>
        /// Percentage price change in last 24 hours.
        /// </summary>
        public double PercentChange24Hour { get; private set; }
        /// <summary>
        /// Percentage price change in last 7 days.
        /// </summary>
        public double PercentChange7Day { get; private set; }
        /// <summary>
        /// Timestamp of when API was last updated.
        /// </summary>
        public int LastUpdated { get; private set; }
        /// <summary>
        /// Fiat currency to convert values to.
        /// </summary>
        public Currencies Currency { get; private set; }
        /// <summary>
        /// Price per unit in specified currency.
        /// </summary>
        public double Price { get; private set; }
        /// <summary>
        /// 24 hour trade volume in specified currency.
        /// </summary>
        public double Volume24Hour { get; private set; }
        /// <summary>
        /// Market capitalization in specified currency.
        /// </summary>
        public double MarketCap { get; private set; }
        /// <summary>
        /// Error thrown by API, if any.
        /// </summary>
        public string Error { get; private set; }

        private Ticker() { }

        internal static async Task<List<Ticker>> GetTickersAsync(Currencies currency = Currencies.USD, int start = 0, int limit = 100)
        {
            Uri uri = GetListUri(currency, start, limit);
            string response = await RestServices.GetResponseAsync(uri).ConfigureAwait(false);
            return await ParseListResponseAsync(response, currency).ConfigureAwait(false);
        }

        internal static async Task<Ticker> GetTickerAsync(string id, Currencies currency = Currencies.USD)
        {
            Uri uri = GetSingleUri(currency, id);
            string response = await RestServices.GetResponseAsync(uri).ConfigureAwait(false);
            return await ParseSingleResponseAsync(response, currency).ConfigureAwait(false);
        }

        private static Uri GetListUri(Currencies currency, int start, int limit)
        {
            string BaseUrl = @"https://api.coinmarketcap.com/v1/ticker/";
            string AdditionFormat = "{0}{1}={2}";
            string UrlAddition = string.Empty;
            bool HasFirstAddition = false;
            if (currency != Currencies.USD)
            {
                UrlAddition += string.Format(AdditionFormat, HasFirstAddition ? "&" : "?", "convert", currency.ToString());
                HasFirstAddition = true;
            }
            if (start != 0)
            {
                UrlAddition += string.Format(AdditionFormat, HasFirstAddition ? "&" : "?", "start", start.ToString());
                HasFirstAddition = true;
            }
            if (limit != 100)
            {
                UrlAddition += string.Format(AdditionFormat, HasFirstAddition ? "&" : "?", "limit", limit.ToString());
                HasFirstAddition = true;
            }
            return new Uri(BaseUrl + UrlAddition);
        }

        private static Uri GetSingleUri(Currencies currency, string id)
        {
            string BaseUrl = @"https://api.coinmarketcap.com/v1/ticker/{0}/{1}";
            string AdditionFormat = "?convert={0}";
            string Addition = string.Empty;
            if (currency != Currencies.USD)
                Addition = string.Format(AdditionFormat, currency.ToString());
            return new Uri(string.Format(BaseUrl, id, Addition));
        }

        private static async Task<List<Ticker>> ParseListResponseAsync(string response, Currencies currency)
        {
            List<Ticker> TickerList = new List<Ticker>();
            Ticker ToAdd = new Ticker();
            ToAdd.Currency = currency;
            using (JsonTextReader jtr = new JsonTextReader(new StringReader(response)))
            {
                while (await jtr.ReadAsync().ConfigureAwait(false))
                {
                    if (jtr.TokenType.ToString() == "StartObject")
                    {
                        if (!string.IsNullOrEmpty(ToAdd.Name))
                        {
                            if (currency == Currencies.USD)
                            {
                                ToAdd.Price = ToAdd.PriceUSD;
                                ToAdd.Volume24Hour = ToAdd.Volume24HourUSD;
                                ToAdd.MarketCap = ToAdd.MarketCapUSD;
                            }
                            TickerList.Add(ToAdd);
                        }
                        ToAdd = new Ticker();
                        ToAdd.Currency = currency;
                        continue;
                    }
                    if (jtr.Value != null)
                    {
                        if (jtr.Value.ToString() == "id")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            ToAdd.ID = jtr.Value.ToString();
                        }
                        else if (jtr.Value.ToString() == "name")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            ToAdd.Name = jtr.Value.ToString();
                        }
                        else if (jtr.Value.ToString() == "symbol")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            ToAdd.Symbol = jtr.Value.ToString();
                        }
                        else if (jtr.Value.ToString() == "rank")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.Rank = Convert.ToInt32(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "price_usd")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PriceUSD = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "price_btc")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PriceBTC = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "24h_volume_usd")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.Volume24HourUSD = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "market_cap_usd")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.MarketCapUSD = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "available_supply")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.AvailableSupply = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "total_supply")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.TotalSupply = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "max_supply")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.MaxSupply = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "percent_change_1h")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PercentChange1Hour = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "percent_change_24h")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PercentChange24Hour = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "percent_change_7d")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PercentChange7Day = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "last_updated")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.LastUpdated = Convert.ToInt32(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "error")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            ToAdd.Error = jtr.Value.ToString();
                        }
                        else if (jtr.Value.ToString() == string.Format("price_{0}", currency.ToString().ToLower()))
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.Price = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == string.Format("24h_volume_{0}", currency.ToString().ToLower()))
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.Volume24Hour = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == string.Format("market_cap_{0}", currency.ToString().ToLower()))
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.MarketCap = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else continue;
                    }
                    else continue;
                }
            }
            if (!string.IsNullOrEmpty(ToAdd.Name))
            {
                if (currency == Currencies.USD)
                {
                    ToAdd.Price = ToAdd.PriceUSD;
                    ToAdd.Volume24Hour = ToAdd.Volume24HourUSD;
                    ToAdd.MarketCap = ToAdd.MarketCapUSD;
                }
                TickerList.Add(ToAdd);
            }
            return TickerList;
        }

        private static async Task<Ticker> ParseSingleResponseAsync(string response, Currencies currency)
        {
            Ticker ToAdd = new Ticker();
            ToAdd.Currency = currency;
            using (JsonTextReader jtr = new JsonTextReader(new StringReader(response)))
            {
                while (await jtr.ReadAsync().ConfigureAwait(false))
                {
                    if (jtr.Value != null)
                    {
                        if (jtr.Value.ToString() == "id")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            ToAdd.ID = jtr.Value.ToString();
                        }
                        else if (jtr.Value.ToString() == "name")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            ToAdd.Name = jtr.Value.ToString();
                        }
                        else if (jtr.Value.ToString() == "symbol")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            ToAdd.Symbol = jtr.Value.ToString();
                        }
                        else if (jtr.Value.ToString() == "rank")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.Rank = Convert.ToInt32(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "price_usd")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PriceUSD = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "price_btc")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PriceBTC = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "24h_volume_usd")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.Volume24HourUSD = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "market_cap_usd")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.MarketCapUSD = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "available_supply")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.AvailableSupply = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "total_supply")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.TotalSupply = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "max_supply")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.MaxSupply = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "percent_change_1h")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PercentChange1Hour = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "percent_change_24h")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PercentChange24Hour = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "percent_change_7d")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.PercentChange7Day = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "last_updated")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.LastUpdated = Convert.ToInt32(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "error")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            ToAdd.Error = jtr.Value.ToString();
                        }
                        else if (jtr.Value.ToString() == string.Format("price_{0}", currency.ToString().ToLower()))
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.Price = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == string.Format("24h_volume_{0}", currency.ToString().ToLower()))
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.Volume24Hour = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == string.Format("market_cap_{0}", currency.ToString().ToLower()))
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            if (jtr.Value != null)
                                ToAdd.MarketCap = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else continue;
                    }
                    else continue;
                }
            }
            if (currency == Currencies.USD)
            {
                ToAdd.Price = ToAdd.PriceUSD;
                ToAdd.Volume24Hour = ToAdd.Volume24HourUSD;
                ToAdd.MarketCap = ToAdd.MarketCapUSD;
            }
            return ToAdd;
        }
    }
}
