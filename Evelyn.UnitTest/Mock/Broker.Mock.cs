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
using Evelyn.Plugin;
using System;
using System.Collections.Generic;

namespace Evelyn.UnitTest.Mock
{
    internal class MockedBroker : IBroker
    {
        private IDictionary<string, IOrderHandler> _orderHandlers = new Dictionary<string, IOrderHandler>();

        public void Delete(string orderID)
        {
            ReceivedDeleteOrders.Add(orderID);
        }

        public void New(NewOrder newOrder, IOrderHandler orderHandler)
        {
            ReceivedNewOrders.Add(newOrder);

            if (_orderHandlers.ContainsKey(newOrder.OrderID))
            {
                throw new ArgumentException("Duplicated order ID.");
            }
            else
            {
                _orderHandlers.Add(newOrder.OrderID, orderHandler);
            }
        }

        #region Mocking Method
        internal void MockedTrade(Trade trade, Description description)
        {
            if (_orderHandlers.TryGetValue(trade.OrderID, out IOrderHandler? handler))
            {
                handler.OnTrade(trade, description);
            }
            else
            {
                throw new ArgumentException("No such order " + trade.OrderID + ".");
            }
        }

        internal List<NewOrder> ReceivedNewOrders { get; } = new List<NewOrder>();
        internal List<string> ReceivedDeleteOrders { get; } = new List<string>();
        #endregion
    }
}