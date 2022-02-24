/*
OHLC for Evelyn Engine, a quantitative trading engine by Chen Hongbao.
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
using PetriSoft.Evelyn.Exceptions;

namespace PetriSoft.Evelyn.Model
{
    /// <summary>
    /// OHLC of the instrument in the specified time span.
    /// </summary>
    public record class OHLC : MarketItem
    {
        private double? _openPrice;
        private double? _highPrice;
        private double? _lowPrice;
        private double? _closePrice;
        private long? _openInterest;
        private long? _volume;
        private TimeSpan? _timeSpan;

        /// <summary>
        /// Open price or the first price within the given time.
        /// </summary>
        public double OpenPrice
        {
            get => _openPrice ?? throw new NullValueException("Open price is null.");
            set => _openPrice = value;
        }

        /// <summary>
        /// Highest price within the given time.
        /// </summary>
        public double HighPrice
        {
            get => _highPrice ?? throw new NullValueException("High price is null.");
            set => _highPrice = value;
        }

        /// <summary>
        /// Lowest price within the given time.
        /// </summary>
        public double LowPrice
        {
            get => _lowPrice ?? throw new NullValueException("Low price is null.");
            set => _lowPrice = value;
        }

        /// <summary>
        /// Close price or the last price within the given time.
        /// </summary>
        public double ClosePrice
        {
            get => _closePrice ?? throw new NullValueException("Close price is null.");
            set => _closePrice = value;
        }

        /// <summary>
        /// Open interest at the moment when the OHLC is completed.
        /// </summary>
        public long OpenInterest
        {
            get => _openInterest ?? throw new NullValueException("Open interest is null.");
            set => _openInterest = value;
        }

        /// <summary>
        /// Sum of trading volume in the given time.
        /// </summary>
        public long Volume
        {
            get => _volume ?? throw new NullValueException("Volume is null.");
            set => _volume = value;
        }

        /// <summary>
        /// The period of time when the OHLC is generated. For example, one-min OHLC has
        /// a time span of 1 minute.
        /// </summary>
        public TimeSpan Time
        {
            get => _timeSpan ?? throw new NullValueException("Time span is null.");
            set => _timeSpan = value;
        }
    }
}
