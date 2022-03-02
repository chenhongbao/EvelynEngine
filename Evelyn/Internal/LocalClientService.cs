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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System.Net;

namespace Evelyn.Internal
{
    internal class LocalClientService : IClientService
    {
        private EngineClientHandler? _clientHandler;
        private readonly Dictionary<string, (IAlgorithm, string[])> _savedClients = new Dictionary<string, (IAlgorithm, string[])>();

        internal EngineClientHandler ClientHandler => _clientHandler ?? throw new NoValueException("Client handler has no value.");

        internal ILogger Logger { get; private init; } = new DebugLoggerProvider().CreateLogger(nameof(LocalClientService));

        public EndPoint? ServiceEndPoint => throw new NotImplementedException("Local client service doesn't have an end point.");

        internal bool IsConfigured { get; private set; } = false;

        public void Configure(IClientHandler clientHandler)
        {
            if (_clientHandler != null)
            {
                throw new InvalidOperationException("Client service cannot be re-configured.");
            }

            _clientHandler = (EngineClientHandler)clientHandler;

            /*
             * Call OnLoad when engine is configured.
             */
            foreach (var clientID in _savedClients.Keys)
            {
                var client = _savedClients[clientID];
                InitializeClient(clientID, client.Item1, client.Item2);
            }
            _savedClients.Clear();
            
            /*
             * Send instruments to clients.
             */
            _clientHandler.FeedHandler.SendInstruments();

            IsConfigured = true;
        }

        public void SendInstrument(Instrument instrument, string clientID)
        {
            try
            {
                ClientHandler[clientID].Algorithm.OnInstrument(instrument);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void SendOHLC(OHLC ohlc, string clientID)
        {
            try
            {
                ClientHandler[clientID].Algorithm.OnFeed(ohlc);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void SendSubscribe(string instrumentID, Description description, bool isSubscribed, string clientID)
        {
            try
            {
                ClientHandler[clientID].Algorithm.OnSubscribed(instrumentID, description, isSubscribed);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void SendTick(Tick tick, string clientID)
        {
            try
            {
                ClientHandler[clientID].Algorithm.OnFeed(tick);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void SendTrade(Trade trade, Description description, string clientID)
        {
            try
            {
                ClientHandler[clientID].Algorithm.OnTrade(trade, description);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        internal void RegisterClient(string clientID, IAlgorithm algorithm, params string[] instrumentID)
        {
            if (IsConfigured)
            {
                throw new InvalidOperationException("Can't register clients after engine is configured.");
            }

            /*
             * Save the client's information and call OnLoad later after engine is configured.
             */
            if (_savedClients.ContainsKey(clientID))
            {
                throw new DuplicatedClientException("Another client exists with ID " + clientID + ".");
            }

            _savedClients.Add(clientID, (algorithm, instrumentID));
        }

        private void InitializeClient(string clientID, IAlgorithm algorithm, params string[] instrumentID)
        {
            ClientHandler.OnClientConnect(clientID, algorithm, this);
            instrumentID.ToList().ForEach(instrument => ClientHandler.OnSubscribe(instrument, true, clientID));

            try
            {
                ClientHandler[clientID].Algorithm.OnLoad(new LocalClientOperator(clientID, ClientHandler));
            }
            catch (Exception ex)
            {
                Logger.LogWarning("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
    }
}