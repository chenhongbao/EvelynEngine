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
    /// New single order.
    /// </summary>
    public record class NewOrder : MarketItem
    {
        private string? _orderID;
        private double? _price;
        private long? _quantity;
        private OrderDirection? _direction;
        private OrderOffset? _offset;

        public string OrderID
        {
            get => _orderID ?? throw new NullValueException("Order ID is null.");
            set => _orderID = value;
        }

        /// <summary>
        /// Price to trade.
        /// </summary>
        public double Price
        {
            get => _price ?? throw new NullValueException("Price is null.");
            set => _price = value;
        }

        /// <summary>
        /// Quantity to trade.
        /// </summary>
        public long Quantity
        {
            get => _quantity ?? throw new NullValueException("Quantity is null.");
            set => _quantity = value;
        }

        /// <summary>
        /// Direction to trade.
        /// </summary>
        public OrderDirection Direction
        {
            get => _direction ?? throw new NullValueException("Direction is null");
            set => _direction = value;
        }

        /// <summary>
        /// Offset to trade.
        /// </summary>
        public OrderOffset Offset
        {
            get => _offset ?? throw new NullValueException("Offset is null.");
            set => _offset = value;
        }
    }
}
