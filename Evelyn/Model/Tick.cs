/*
Market data tick for Evelyn Engine, a quantitative trading engine by Chen Hongbao.
Copyright (C) 2022  Chen Hongbao<chenhongbao@outlook.com>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace PetriSoft.Evelyn.Model
{
    /// <summary>
    /// Tick or market data snapshot for an instrument.
    /// </summary>
    public record class Tick : MarketItem
    {
        private long? _openInterest;
        private long? _preOpenInterest;
        private double? _preClosePrice;
        private double? _preSettlementPrice;

        /// <summary>
        /// Average price for current trading day.
        /// </summary>
        public double? AveragePrice { get; set; }

        /// <summary>
        /// Last trading price of the instrumentl or null Iif there is no trade in the current 
        /// trading day.
        /// </summary>
        public double? LastPrice { get; set; }

        /// <summary>
        /// Open price of the current trading day, or null if the instrument hasn't been opened
        /// in the current trading day.
        /// </summary>
        public double? OpenPrice { get; set; }

        /// <summary>
        /// Highest price of the current trading day, or null if the instrument hasn't been 
        /// traded in the current day.
        /// </summary>
        public double? HighPrice { get; set; }

        /// <summary>
        /// Lowest price of the current trading day, or null if the instrument hasn't been 
        /// traded in the current day.
        /// </summary>
        public double? LowPrice { get; set; }

        /// <summary>
        /// Close price of the current trading day, or null if the instrument is not closed yet in
        /// the current trading day.
        /// </summary>
        public double? ClosePrice { get; set; }

        /// <summary>
        /// Highest price of the current trading day, or null if the instrument is not settled yet in
        /// the current trading day.
        /// </summary>
        public double? SettlementPrice { get; set; }

        /// <summary>
        /// Total trading valume of the current trading day, or null is there is no trade yet.
        /// </summary>
        public long? Volume { get; set; }

        /// <summary>
        /// Open interest of the instrument.
        /// </summary>
        public long OpenInterest
        {
            get => _openInterest ?? throw new NullValueException("Open interest is null.");
            set => _openInterest = value;
        }

        /// <summary>
        /// Close price of the instrument yesterday.
        /// </summary>
        public double PreClosePrice
        {
            get => _preClosePrice ?? throw new NullValueException("Pre close price is null.");
            set => _preClosePrice = value;
        }

        /// <summary>
        /// Settlement price of the instrument yesterday.
        /// </summary>
        public double PreSettlementPrice
        {
            get => _preSettlementPrice ?? throw new NullValueException("Pre settlement price is null.");
            set => _preSettlementPrice = value;
        }

        /// <summary>
        /// Open interest of the instrument yesterday.
        /// </summary>
        public long PreOpenInterest
        {
            get => _preOpenInterest ?? throw new NullValueException("Pre open interest is null.");
            set => _preOpenInterest = value;
        }

        /// <summary>
        /// Spread summary of the snapshot at the time when this tick is generated.
        /// </summary>
        public IReadOnlyList<TickSpread> Spreads { get; set; } = new List<TickSpread>();
    }
}
