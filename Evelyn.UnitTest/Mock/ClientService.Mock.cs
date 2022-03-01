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
        private IClientHandler? _clientHandler;
        private IDictionary<string, MockedClient> _clients = new Dictionary<string, MockedClient>();

        internal IClientHandler ClientHandler
        {
            get => _clientHandler ?? throw new NoValueException("Client order handler has no value.");
            private set => _clientHandler = value;
        }

        public EndPoint? ServiceEndPoint => throw new System.NotImplementedException();

        public void Service(IClientHandler orderHandler)
        {
            ClientHandler = orderHandler;
        }

        public void SendInstrument(Instrument instrument, string clientID)
        {
            GetClient(clientID).ReceivedInstruments.Add(instrument);
        }

        public void SendOHLC(OHLC ohlc, string clientID)
        {
            GetClient(clientID).ReceivedOHLCs.Add(ohlc);
        }

        public void SendSubscribe(string instrumentID, Description description, bool isSubscribed, string clientID)
        {
            GetClient(clientID).ReceivedSubscribe = (instrumentID, description, isSubscribed);
        }

        public void SendTick(Tick tick, string clientID)
        {
            GetClient(clientID).ReceivedTicks.Add(tick);
        }

        public void SendTrade(Trade trade, Description description, string clientID)
        {
            GetClient(clientID).ReceivedTrades.Add((trade, description));
        }

        private MockedClient GetClient(string clientID)
        {
            return _clients[clientID];
        }

        #region Mocking Methods
        internal void MockedSubscribe(string instrumentID, bool isSubscribed, string clientID)
        {
            ClientHandler.OnSubscribe(instrumentID, isSubscribed, clientID);
        }

        internal void MockedNewOrder(NewOrder newOrder, string clientID)
        {
            ClientHandler.OnNewOrder(newOrder, clientID);
        }

        internal MockedClient GetClientOrCreate(string clientID)
        {
            if (!_clients.ContainsKey(clientID))
            {
                _clients.Add(clientID, new MockedClient());
            }
            return _clients[clientID];
        }

        internal void MockedDelete(string orderID, string clientID)
        {
            ClientHandler.OnDeleteOrder(orderID, clientID);
        }
        #endregion
    }

    internal class MockedClient
    {
        internal List<Tick> ReceivedTicks { get; } = new List<Tick>();
        internal List<OHLC> ReceivedOHLCs { get; } = new List<OHLC>();
        internal List<Instrument> ReceivedInstruments { get; } = new List<Instrument>();
        internal (string, Description, bool) ReceivedSubscribe { get; set; }
        internal List<(Trade, Description)> ReceivedTrades { get; } = new List<(Trade, Description)>();
    }
}

