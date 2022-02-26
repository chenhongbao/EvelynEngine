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
    public struct Tick
    {
        private string? _instrumentID = null;
        private DateOnly? _tradingDay = null;
        private DateTime? _timeStamp = null;
        private double? _lastPrice = null;
        private long? _volume = null;
        private long? _openInterest = null;
        private long? _preOpenInterest = null;
        private double? _preClosePrice = null;
        private double? _preSettlementPrice = null;

        public Tick()
        {
        }

        public string InstrumentID
        {
            get => _instrumentID ?? throw new NullValueException("Instrument ID is null.");
            set => _instrumentID = value;
        }

        public string ExchangeID { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;


        public DateOnly TradingDay
        {
            get => _tradingDay ?? throw new NullValueException("Trading day is null.");
            set => _tradingDay = value;
        }

        public DateTime TimeStamp
        {
            get => _timeStamp ?? throw new NullValueException("Timestamp is null.");
            set => _timeStamp = value;
        }

        public double? AveragePrice { get; set; } = null;

        public double LastPrice
        {
            get => _lastPrice ?? throw new NullValueException("Last price is null");
            set => _lastPrice = value;
        }

        public double? OpenPrice { get; set; } = null;

        public double? HighPrice { get; set; } = null;

        public double? LowPrice { get; set; } = null;

        public double? ClosePrice { get; set; } = null;

        public double? SettlementPrice { get; set; } = null;

        public long Volume
        {
            get => _volume ?? throw new NullValueException("Volume is null.");
            set => _volume = value;
        }

        public long OpenInterest
        {
            get => _openInterest ?? throw new NullValueException("Open interest is null.");
            set => _openInterest = value;
        }

        public double PreClosePrice
        {
            get => _preClosePrice ?? throw new NullValueException("Pre close price is null.");
            set => _preClosePrice = value;
        }

        public double PreSettlementPrice
        {
            get => _preSettlementPrice ?? throw new NullValueException("Pre settlement price is null.");
            set => _preSettlementPrice = value;
        }

        public long PreOpenInterest
        {
            get => _preOpenInterest ?? throw new NullValueException("Pre open interest is null.");
            set => _preOpenInterest = value;
        }

        public IReadOnlyList<TickSpread> Spreads { get; set; } = new List<TickSpread>();
    }
}
