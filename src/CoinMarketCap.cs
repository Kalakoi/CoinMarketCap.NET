using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kalakoi.Crypto.CoinMarketCap
{
    /// <summary>
    /// Provides access to the CoinMarketCap service.
    /// </summary>
    public static class CoinMarketCap
    {
        /// <summary>
        /// Gets global data about all curencies and assets.
        /// </summary>
        /// <param name="Currency">Fiat currency to convert to.</param>
        /// <returns>Object containing global data.</returns>
        public static async Task<GlobalData> GetGlobalData(Currencies Currency = Currencies.USD) =>
            await GlobalData.GetDataAsync(Currency).ConfigureAwait(false);

        /// <summary>
        /// Gets all available tickers with accompanying data.
        /// </summary>
        /// <param name="Currency">Fiat currency to convert to.</param>
        /// <returns>List of all tickers with data.</returns>
        public static async Task<List<Ticker>> GetAllTickersAsync(Currencies Currency = Currencies.USD) =>
            await Ticker.GetTickersAsync(Currency, 0, 0).ConfigureAwait(false);

        /// <summary>
        /// Gets a selection of tickers with accompanying data.
        /// </summary>
        /// <param name="Currency">Fiat currency to convert to.</param>
        /// <param name="Start">Rank to start from.</param>
        /// <param name="Limit">Number of tickers to return.</param>
        /// <returns>List of tickers with data.</returns>
        public static async Task<List<Ticker>> GetTickersAsync(Currencies Currency = Currencies.USD, int Start = 0, int Limit = 100) =>
            await Ticker.GetTickersAsync(Currency, Start, Limit).ConfigureAwait(false);

        /// <summary>
        /// Gets specific ticker with accompanying data.
        /// </summary>
        /// <param name="ID">ID of ticker to pull.</param>
        /// <param name="Currency">Fiat currency to convert to.</param>
        /// <returns>Ticker data.</returns>
        public static async Task<Ticker> GetTickerAsync(string ID, Currencies Currency = Currencies.USD) =>
            await Ticker.GetTickerAsync(ID, Currency).ConfigureAwait(false);

        /// <summary>
        /// Gets currency ID from ticker symbol.
        /// </summary>
        /// <param name="Symbol">Ticker symbol.</param>
        /// <returns>Currency ID.</returns>
        public static async Task<string> GetIDFromSymbol(string Symbol)
        {
            foreach (Ticker t in await GetAllTickersAsync().ConfigureAwait(false))
                if (t.Symbol == Symbol)
                    return t.ID;
            return string.Empty;
        }
    }
}
