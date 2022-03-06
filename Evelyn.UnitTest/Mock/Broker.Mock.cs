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
        private IOrderHandler? _orderHandlers;
        private IExchangeListener? _exchangeListener;

        private IOrderHandler Handler => _orderHandlers ?? throw new NullReferenceException("Order handler has no value.");

        public void Delete(DeleteOrder deleteOrder)
        {
            ReceivedDeleteOrders.Add(deleteOrder.OrderID);
        }

        public void New(NewOrder newOrder)
        {
            ReceivedNewOrders.Add(newOrder);
        }

        public void Register(IOrderHandler orderHandler, IExchangeListener exchangeListener)
        {
            _orderHandlers = orderHandler;
            _exchangeListener = exchangeListener;
        }

        #region Mocking Method
        internal void MockedTrade(Trade trade, Description description)
        {
            Handler.OnTrade(trade, description);
        }

        internal void MockedConnect(bool isConnected)
        {
            (_exchangeListener ?? throw new NullReferenceException("Exchange listener has no value.")).OnConnected(isConnected);
        }

        internal List<NewOrder> ReceivedNewOrders { get; } = new List<NewOrder>();
        internal List<string> ReceivedDeleteOrders { get; } = new List<string>();

        public string NewOrderID => Guid.NewGuid().ToString("N");

        public DateOnly TradingDay { get; set; } = DateOnly.MaxValue;
        #endregion
    }
}