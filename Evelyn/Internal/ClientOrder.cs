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
using Evelyn.Model;
using System.Collections.Concurrent;

namespace Evelyn.Internal
{
    internal class ClientOrder
    {
        private readonly NewOrder _order;
        private readonly string _borkerOrderID;

        internal ClientOrder(NewOrder newOrder, string brokerOrderID)
        {
            _order = newOrder;
            _borkerOrderID = brokerOrderID;
            Status = OrderStatus.None;
        }

        internal DeleteOrder RewriteDeleteOrder
        {
            get
            {
                return new DeleteOrder
                {
                    OrderID = _borkerOrderID,
                    InstrumentID = _order.InstrumentID
                };
            }
        }

        internal NewOrder RewriteNewOrder
        {
            get
            {
                var copied = _order;

                copied.OrderID = _borkerOrderID;
                return copied;
            }
        }

        internal NewOrder OriginalOrder => _order;
        internal string BrokerOrderID => _borkerOrderID;
        internal OrderStatus Status { get; set; }
        internal ConcurrentQueue<Trade> Trades { get; init; } = new ConcurrentQueue<Trade>();
    }
}