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
        private long? _quantity = null;
        private OrderDirection? _direction = null;
        private OrderOffset? _offset = null;
        private string? _tradeID = null;
        private double? _tradePrice = null;
        private long? _tradeQuantity = null;
        private long? _leaveQuantity = null;
        private DateTime? _tradeTimeStamp = null;
        private OrderStatus? _status = null;
        private string? _message = null;

        public Trade()
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

        public string OrderID
        {
            get => _orderID ?? throw new NoValueException("Order ID has no value.");
            set => _orderID = value;
        }

        public double Price
        {
            get => _price ?? throw new NoValueException("Price has no value.");
            set => _price = value;
        }

        public long Quantity
        {
            get => _quantity ?? throw new NoValueException("Quantity has no value.");
            set => _quantity = value;
        }

        public OrderDirection Direction
        {
            get => _direction ?? throw new NoValueException("Direction has no value");
            set => _direction = value;
        }

        public OrderOffset Offset
        {
            get => _offset ?? throw new NoValueException("Offset has no value.");
            set => _offset = value;
        }

        public string TradeID
        {
            get => _tradeID ?? throw new NoValueException("Trade ID has no value.");
            set => _tradeID = value;
        }

        public double TradePrice
        {
            get => _tradePrice ?? throw new NoValueException("Trade price has no value.");
            set => _tradePrice = value;
        }

        public long TradeQuantity
        {
            get => _tradeQuantity ?? throw new NoValueException("Trade quantity has no value.");
            set => _tradeQuantity = value;
        }

        public long LeaveQuantity
        {
            get => _leaveQuantity ?? throw new NoValueException("Leave quantity has no value.");
            set => _leaveQuantity = value;
        }

        public DateTime TradeTimeStamp
        {
            get => _tradeTimeStamp ?? throw new NoValueException("Trade time has no value.");
            set => _tradeTimeStamp = value;
        }

        public OrderStatus Status
        {
            get => _status ?? throw new NoValueException("Trade status has no value.");
            set => _status = value;
        }

        public string Message
        {
            get => _message ?? throw new NoValueException("Message has no value.");
            set => _message = value;
        }
    }
}
