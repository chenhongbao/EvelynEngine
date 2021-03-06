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
    public struct Trade
    {
        private string? _instrumentID = null;
        private DateOnly? _tradingDay = null;
        private DateTime? _timeStamp = null;
        private string? _orderID = null;
        private double? _price = null;
        private int? _quantity = null;
        private Direction? _direction = null;
        private Offset? _offset = null;
        private string? _tradeID = null;
        private double? _tradePrice = null;
        private int? _tradeQuantity = null;
        private int? _leaveQuantity = null;
        private OrderStatus? _status = null;

        public Trade()
        {
        }

        public string InstrumentID
        {
            get => _instrumentID ?? throw new NullReferenceException("Instrument ID has no value.");
            set => _instrumentID = value;
        }

        public string ExchangeID { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;


        public DateOnly TradingDay
        {
            get => _tradingDay ?? throw new NullReferenceException("Trading day has no value.");
            set => _tradingDay = value;
        }

        public DateTime TimeStamp
        {
            get => _timeStamp ?? throw new NullReferenceException("Timestamp has no value.");
            set => _timeStamp = value;
        }

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

        public string TradeID
        {
            get => _tradeID ?? throw new NullReferenceException("Trade ID has no value.");
            set => _tradeID = value;
        }

        public double TradePrice
        {
            get => _tradePrice ?? throw new NullReferenceException("Trade price has no value.");
            set => _tradePrice = value;
        }

        public int TradeQuantity
        {
            get => _tradeQuantity ?? throw new NullReferenceException("Trade quantity has no value.");
            set => _tradeQuantity = value;
        }

        public int LeaveQuantity
        {
            get => _leaveQuantity ?? throw new NullReferenceException("Leave quantity has no value.");
            set => _leaveQuantity = value;
        }

        public OrderStatus Status
        {
            get => _status ?? throw new NullReferenceException("Trade status has no value.");
            set => _status = value;
        }

        public string Message { get; set; } = string.Empty;
    }
}
