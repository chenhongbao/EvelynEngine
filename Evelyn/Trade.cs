/*
Event of specified instrument for Evelyn Engine, a quantitative trading engine by Chen Hongbao.
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
namespace PetriSoft.Evelyn
{
    /// <summary>
    /// A trade report of the order.
    /// <para>An order may have many trades, hence trade reports, that are filled separately. </para>
    /// </summary>
    public record class Trade : NewOrder
    {
        private string? _tradeID;
        private double? _tradePrice;
        private long? _tradeQuantity;
        private long? _leaveQuantity;
        private DateTime? _tradeTimeStamp;
        private OrderStatus? _status;
        private string? _message;

        public string TradeID
        {
            get => _tradeID ?? throw new NullValueException("Trade ID is null.");
            set => _tradeID = value;
        }

        /// <summary>
        /// Traded price of the latest fill.
        /// </summary>
        public double TradePrice
        {
            get => _tradePrice ?? throw new NullValueException("Trade price is null.");
            set => _tradePrice = value;
        }

        /// <summary>
        /// Traded quantity of the latest fill.
        /// </summary>
        public long TradeQuantity
        {
            get => _tradeQuantity ?? throw new NullValueException("Trade quantity is null.");
            set => _tradeQuantity = value;
        }

        /// <summary>
        /// Quantity not filled.
        /// </summary>
        public long LeaveQuantity
        {
            get => _leaveQuantity ?? throw new NullValueException("Leave quantity is null.");
            set => _leaveQuantity = value;
        }

        /// <summary>
        /// Time stamp of the latest fill.
        /// </summary>
        public DateTime TradeTimeStamp
        {
            get => _tradeTimeStamp ?? throw new NullValueException("Trade time is null.");
            set => _tradeTimeStamp = value;
        }

        /// <summary>
        /// Status of the order.
        /// </summary>
        public OrderStatus Status
        {
            get => _status ?? throw new NullValueException("Trade status is null.");
            set => _status = value;
        }

        /// <summary>
        /// Message for this trade report.
        /// </summary>
        public string Message
        {
            get => _message ?? throw new NullValueException("Message is null.");
            set => _message = value;
        }
    }
}
