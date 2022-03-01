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
using System.Net;

namespace Evelyn.Internal
{
    internal class LocalClientService : IClientService
    {
        private EngineClientHandler? _clientHandler;

        internal EngineClientHandler ClientHandler => _clientHandler ?? throw new NoValueException("Client handler has no value.");

        public EndPoint? ServiceEndPoint => throw new NotImplementedException("Local client service doesn't have an end point.");

        public void Service(IClientHandler clientHandler)
        {
            _clientHandler = (EngineClientHandler)clientHandler;
        }

        public void SendInstrument(Instrument instrument, string clientID)
        {
            ClientHandler[clientID].Algorithm.OnInstrument(instrument);
        }

        public void SendOHLC(OHLC ohlc, string clientID)
        {
            ClientHandler[clientID].Algorithm.OnFeed(ohlc);
        }

        public void SendSubscribe(string instrumentID, Description description, bool isSubscribed, string clientID)
        {
            ClientHandler[clientID].Algorithm.OnSubscribed(instrumentID, description, isSubscribed);
        }

        public void SendTick(Tick tick, string clientID)
        {
            ClientHandler[clientID].Algorithm.OnFeed(tick);
        }

        public void SendTrade(Trade trade, Description description, string clientID)
        {
            ClientHandler[clientID].Algorithm.OnTrade(trade, description);
        }

        internal void EnableClient(string clientID, IAlgorithm algorithm, params string[] instrumentID)
        {
            ClientHandler.OnClientConnect(clientID, algorithm, this);
            instrumentID.ToList().ForEach(instrument => ClientHandler.OnSubscribe(instrument, true, clientID));
        }
    }
}