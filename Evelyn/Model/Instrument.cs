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
namespace Evelyn.Model
{
    public struct Instrument
    {
        private string? _instrumentID = null;
        private DateOnly? _tradingDay = null;
        private InstrumentState? _instrumentState = null;
        private DateTime? _stateTimestamp = null;

        public Instrument()
        {
        }

        public string InstrumentID
        {
            get => _instrumentID ?? throw new NullReferenceException("Instrument ID has no value.");
            set => _instrumentID = value;
        }

        public string ExchangeID { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;


        public DateOnly? TradingDay
        {
            get => _tradingDay;
            set => _tradingDay = value;
        }

        public InstrumentState? State
        {
            get => _instrumentState;
            set => _instrumentState = value;
        }

        public DateTime? StateTimestamp
        {
            get => _stateTimestamp;
            set => _stateTimestamp = value;
        }
    }
}