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
        public void Delete(string orderID)
        {
            throw new System.NotImplementedException();
        }

        public void New(NewOrder newOrder, IOrderHandler orderHandler)
        {
            throw new System.NotImplementedException();
        }

        #region Mocking Method
        internal void MockedTrade(Trade trade, Description description)
        {
            throw new NotImplementedException();
        }

        internal List<NewOrder> ReceivedNewOrders { get; } = new List<NewOrder>();
        internal List<string> ReceivedDeleteOrders { get; } = new List<string>();
        #endregion
    }
}