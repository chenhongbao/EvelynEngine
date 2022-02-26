/*
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
    public struct OHLC
    {
        private string? _instrumentID = null;
        private DateOnly? _tradingDay = null;
        private DateTime? _timeStamp = null;
        private double? _openPrice = null;
        private double? _highPrice = null;
        private double? _lowPrice = null;
        private double? _closePrice = null;
        private long? _openInterest = null;
        private long? _volume = null;
        private TimeSpan? _timeSpan = null;

        public OHLC()
        {
        }

        public string InstrumentID
        {
            get => _instrumentID ?? throw new NoValueException("Instrument ID has no value.");
            set => _instrumentID = value;
        }

        public string ExchangeID { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;


        public DateOnly TradingDay
        {
            get => _tradingDay ?? throw new NoValueException("Trading day has no value.");
            set => _tradingDay = value;
        }

        public DateTime TimeStamp
        {
            get => _timeStamp ?? throw new NoValueException("Timestamp has no value.");
            set => _timeStamp = value;
        }

        public double OpenPrice
        {
            get => _openPrice ?? throw new NoValueException("Open price has no value.");
            set => _openPrice = value;
        }

        public double HighPrice
        {
            get => _highPrice ?? throw new NoValueException("High price has no value.");
            set => _highPrice = value;
        }

        public double LowPrice
        {
            get => _lowPrice ?? throw new NoValueException("Low price has no value.");
            set => _lowPrice = value;
        }

        public double ClosePrice
        {
            get => _closePrice ?? throw new NoValueException("Close price has no value.");
            set => _closePrice = value;
        }

        public long OpenInterest
        {
            get => _openInterest ?? throw new NoValueException("Open interest has no value.");
            set => _openInterest = value;
        }

        public long Volume
        {
            get => _volume ?? throw new NoValueException("Volume has no value.");
            set => _volume = value;
        }

        public TimeSpan Time
        {
            get => _timeSpan ?? throw new NoValueException("Time span has no value.");
            set => _timeSpan = value;
        }
    }
}
