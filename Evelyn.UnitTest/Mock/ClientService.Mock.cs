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
using System;
using System.Collections.Generic;
using System.Net;

namespace Evelyn.UnitTest.Mock
{
    internal class MockedClientService : IClientService
    {
        public EndPoint? ServiceEndPoint => throw new System.NotImplementedException();

        public void ReceiveOrder(IClientOrderHandler orderHandler)
        {
            throw new System.NotImplementedException();
        }

        public void ReceiveSubscribe(IClientSubscriptionHandler subscriptionHandler)
        {
            throw new System.NotImplementedException();
        }

        public void SendInstrument(Instrument instrument, string clientID)
        {
            throw new System.NotImplementedException();
        }

        public void SendOHLC(OHLC ohlc, string clientID)
        {
            throw new System.NotImplementedException();
        }

        public void SendSubscribe(string instrumentID, Description description, bool isSubscribed, string clientID)
        {
            throw new System.NotImplementedException();
        }

        public void SendTick(Tick tick, string clientID)
        {
            throw new System.NotImplementedException();
        }

        public void SendTrade(Trade trade, Description description, string clientID)
        {
            throw new System.NotImplementedException();
        }

        #region Mocking Methods
        internal void MockedSubscribe(string instrumentID, bool isSubscribed, string clientID)
        {
            throw new NotImplementedException();
        }

        internal void MockedNewOrder(NewOrder newOrder, string clientID)
        {
            throw new NotImplementedException();
        }

        internal MockedClient GetClientOrCreate(string clientID)
        {
            throw new NotImplementedException();
        }

        internal void MockedDelete(string orderID, string clientID)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    internal class MockedClient
    {
        internal List<Tick> ReceivedTicks { get; } = new List<Tick>();
        internal List<OHLC> ReceivedOHLCs { get; } = new List<OHLC>();
        internal List<Instrument> ReceivedInstruments { get; } = new List<Instrument>();
        internal (string, Description, bool) ReceivedSubscribe { get; private set; }
        internal List<(Trade, Description)> ReceivedTrades { get; } = new List<(Trade, Description)>();
    }
}

