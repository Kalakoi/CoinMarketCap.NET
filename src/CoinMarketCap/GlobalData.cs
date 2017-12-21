using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kalakoi.Crypto.CoinMarketCap
{
    /// <summary>
    /// Provides access to global data provided by CoinMarketCap
    /// </summary>
    public class GlobalData
    {
        /// <summary>
        /// Market capitalization in USD.
        /// </summary>
        public double TotalMarketCapUSD { get; private set; }
        /// <summary>
        /// 24 Hour trade volume in USD.
        /// </summary>
        public double Total24HourVolumeUSD { get; private set; }
        /// <summary>
        /// Bitcoin percentage dominance.
        /// </summary>
        public double BitcoinPercentageOfMarketCap { get; private set; }
        /// <summary>
        /// Number of active currencies on CoinMarketCap.
        /// </summary>
        public int ActiveCurrencies { get; private set; }
        /// <summary>
        /// Number of active assets on CoinMarketCap.
        /// </summary>
        public int ActiveAssets { get; private set; }
        /// <summary>
        /// Number of active markets on CoinMarketCap.
        /// </summary>
        public int ActiveMarkets { get; private set; }
        /// <summary>
        /// Timestamp for when API was last updated.
        /// </summary>
        public int LastUpdated { get; private set; }
        /// <summary>
        /// Currency specified for value conversions.
        /// </summary>
        public Currencies Currency { get; private set; }
        /// <summary>
        /// Market capitalization in specified currency.
        /// </summary>
        public double TotalMarketCap { get; private set; }
        /// <summary>
        /// 24 hour trade volume in specified currency.
        /// </summary>
        public double Total24HourVolume { get; private set; }

        private GlobalData() { }

        internal static async Task<GlobalData> GetDataAsync(Currencies currency = Currencies.USD)
        {
            Uri uri = GetUri(currency);
            string response = await RestServices.GetResponseAsync(uri).ConfigureAwait(false);
            return await ParseResponseAsync(response, currency).ConfigureAwait(false);
        }

        private static Uri GetUri(Currencies currency)
        {
            string BaseUrl = @"https://api.coinmarketcap.com/v1/global/{0}";
            string AdditionFormat = "?convert={0}";
            string Addition = string.Empty;
            if (currency != Currencies.USD)
                Addition = string.Format(AdditionFormat, currency.ToString());
            return new Uri(string.Format(BaseUrl, Addition));
        }

        private static async Task<GlobalData> ParseResponseAsync(string response, Currencies currency)
        {
            GlobalData data = new GlobalData();
            data.Currency = currency;
            using (JsonTextReader jtr = new JsonTextReader(new StringReader(response)))
            {
                while (await jtr.ReadAsync().ConfigureAwait(false))
                {
                    if (jtr.Value != null)
                    {
                        if (jtr.Value.ToString() == "total_market_cap_usd")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            data.TotalMarketCapUSD = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "total_24h_volume_usd")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            data.Total24HourVolumeUSD = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "bitcoin_percentage_of_market_cap")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            data.BitcoinPercentageOfMarketCap = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "active_currencies")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            data.ActiveCurrencies = Convert.ToInt32(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "active_assets")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            data.ActiveAssets = Convert.ToInt32(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "active_markets")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            data.ActiveMarkets = Convert.ToInt32(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == "last_updated")
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            data.LastUpdated = Convert.ToInt32(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == string.Format("total_market_cap_{0}", currency.ToString().ToLower()))
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            data.TotalMarketCap = Convert.ToDouble(jtr.Value.ToString());
                        }
                        else if (jtr.Value.ToString() == string.Format("total_24h_volume_{0}", currency.ToString().ToLower()))
                        {
                            await jtr.ReadAsync().ConfigureAwait(false);
                            data.Total24HourVolume = Convert.ToDouble(jtr.Value.ToString());
                        }
                    }
                    else continue;
                }
            }
            if (currency == Currencies.USD)
            {
                data.TotalMarketCap = data.TotalMarketCapUSD;
                data.Total24HourVolume = data.Total24HourVolumeUSD;
            }
            return data;
        }
    }
}
