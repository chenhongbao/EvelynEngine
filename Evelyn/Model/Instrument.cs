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
    public struct Instrument
    {
        private string? _instrumentID = null;
        private DateOnly? _tradingDay = null;
        private DateTime? _timeStamp = null;
        private double? _margin = null;
        private double? _commission = null;
        private long? _multiple = null;
        private CalculationMethod? _marginMethod = null;
        private CalculationMethod? _commissionMethod = null;
        private InstrumentState? _instrumentState = null;
        private DateTime? _stateTimestamp = null;

        public Instrument()
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

        public double Margin
        {
            get => _margin ?? throw new NullValueException("Margin rate is null.");
            set => _margin = value;
        }

        public double Commission
        {
            get => _commission ?? throw new NullValueException("Commission rate is null.");
            set => _commission = value;
        }

        public long Multiple
        {
            get => _multiple ?? throw new NullValueException("Multiple is null.");
            set => _multiple = value;
        }

        public CalculationMethod MarginMethod
        {
            get => _marginMethod ?? throw new NullValueException("Margin type is null.");
            set => _marginMethod = value;
        }

        public CalculationMethod CommissionMethod
        {
            get => _commissionMethod ?? throw new NullValueException("Margin type is null.");
            set => _commissionMethod = value;
        }

        public InstrumentState State
        {
            get => _instrumentState ?? throw new NullValueException("Instrument trading state is null.");
            set => _instrumentState = value;
        }

        public DateTime StateTimestamp
        {
            get => _stateTimestamp ?? throw new NullValueException("State timestamp is null.");
            set => _stateTimestamp = value;
        }
    }
}