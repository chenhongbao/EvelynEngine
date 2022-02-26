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
    public struct Contract
    {
        private string? _instrumentID = null;
        private DateOnly? _tradingDay = null;
        private DateTime? _timeStamp = null;
        private double? _price = null;
        private ContractStatus? _status = null;

        public Contract()
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

        public ContractStatus Status
        {
            get => _status ?? throw new NoValueException("Contract status has no value.");
            set => _status = value;
        }

        public double Price
        {
            get => _price ?? throw new NoValueException("Price has no value.");
            set => _price = value;
        }

        public double? ClosePrice { get; set; } = null;

        public DateTime? CloseTimeStamp { get; set; } = null;

        public DateOnly? CloseTradingDay { get; set; } = null;
    }
}