/*
Null value exception for Evelyn Engine, a quantitative trading engine by Chen Hongbao.
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
    /// Contract.
    /// </summary>
    public record class Contract : MarketItem
    {
        private double? _price;
        private ContractStatus? _status;

        /// <summary>
        /// Contract status.
        /// </summary>
        public ContractStatus Status
        {
            get => _status ?? throw new NullValueException("Contract status is  null.");
            set => _status = value;
        }

        /// <summary>
        /// Contract open price.
        /// </summary>
        public double Price
        {
            get => _price ?? throw new NullValueException("Price is null.");
            set => _price = value;
        }

        /// <summary>
        /// Contract close price. <c>null</c> if the contract is not being closed nor closed.
        /// </summary>
        public double? ClosePrice { get; set; }

        /// <summary>
        /// Contract close time stamp. <c>null</c> if the contract is not being closed nor closed.
        /// </summary>
        public DateTime? CloseTimeStamp { get; set; }

        /// <summary>
        /// Contract close trading day. <c>null</c> if the contract is not being closed nor closed.
        /// </summary>
        public DateOnly? CloseTradingDay { get; set; }
    }
}