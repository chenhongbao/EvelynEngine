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
    public struct NewOrder
    {
        private string? _instrumentID = null;
        private string? _exchangeID = null;
        private string? _orderID = null;
        private double? _price = null;
        private int? _quantity = null;
        private Direction? _direction = null;
        private Offset? _offset = null;

        public NewOrder()
        {
        }

        public string InstrumentID
        {
            get => _instrumentID ?? throw new NullReferenceException("Instrument ID has no value.");
            set => _instrumentID = value;
        }

        public string ExchangeID
        {
            get => _exchangeID ?? throw new NullReferenceException("Exchange ID has no value.");
            set => _exchangeID = value;
        }

        public string Symbol { get; set; } = string.Empty;


        public DateOnly? TradingDay { get; set; } = null;

        public DateTime? TimeStamp { get; set; } = null;

        public string OrderID
        {
            get => _orderID ?? throw new NullReferenceException("Order ID has no value.");
            set => _orderID = value;
        }

        public double Price
        {
            get => _price ?? throw new NullReferenceException("Price has no value.");
            set => _price = value;
        }

        public int Quantity
        {
            get => _quantity ?? throw new NullReferenceException("Quantity has no value.");
            set => _quantity = value;
        }

        public Direction Direction
        {
            get => _direction ?? throw new NullReferenceException("Direction has no value");
            set => _direction = value;
        }

        public Offset Offset
        {
            get => _offset ?? throw new NullReferenceException("Offset has no value.");
            set => _offset = value;
        }
    }
}
