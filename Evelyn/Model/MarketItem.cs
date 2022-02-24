/*
Market item for Evelyn Engine, a quantitative trading engine by Chen Hongbao.
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
    public record class MarketItem
    {
        private string? _instrumentID;
        private DateOnly? _tradingDay;
        private DateTime? _timeStamp;

        /// <summary>
        /// Instrument ID which must be presented to broker for new order.
        /// </summary>
        public string InstrumentID
        {
            get => _instrumentID ?? throw new NullValueException("Instrument ID is null.");
            set => _instrumentID = value;
        }

        /// <summary>
        /// Exchange ID of the instrument.
        /// </summary>
        public string? ExchangeID { get; set; }

        /// <summary>
        /// Symbol of the instrument.
        /// </summary>
        public string? Symbol { get; set; }


        /// <summary>
        /// Trading day of this item.
        /// </summary>
        public DateOnly TradingDay
        {
            get => _tradingDay ?? throw new NullValueException("Trading day is null.");
            set => _tradingDay = value;
        }

        /// <summary>
        /// Time stamp of this item.
        /// </summary>
        public DateTime TimeStamp
        {
            get => _timeStamp ?? throw new NullValueException("Timestamp is null.");
            set => _timeStamp = value;
        }
    }
}