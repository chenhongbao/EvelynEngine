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
using Evelyn.Internal.Logging;
using Evelyn.Model;
using Evelyn.Plugin;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Evelyn.Internal
{
    internal class LocalClientService : IClientService
    {
        private EngineClientHandler? _clientHandler;
        private readonly ConcurrentDictionary<string, ConfiguringClient> _savedClients = new ConcurrentDictionary<string, ConfiguringClient>();

        internal EngineClientHandler ClientHandler => _clientHandler ?? throw new NullReferenceException("Client handler has no value.");
        internal ILogger Logger { get; private init; } = Loggers.CreateLogger(nameof(LocalClientService));
        private bool IsConfigured { get; set; } = false;

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
                InitializeClient(client.ClientID, client.Algorithm, client.Instruments);
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
            CallClientMethod(clientID, client => client.Algorithm.OnFeed(instrument));
        }

        public void SendOHLC(OHLC ohlc, string clientID)
        {
            CallClientMethod(clientID, client => client.Algorithm.OnFeed(ohlc));
        }

        public void SendSubscribe(string instrumentID, Description description, bool isSubscribed, string clientID)
        {
            CallClientMethod(clientID, client => client.Algorithm.OnSubscribed(instrumentID, description, isSubscribed));
        }

        public void SendTick(Tick tick, string clientID)
        {
            CallClientMethod(clientID, client => client.Algorithm.OnFeed(tick));
        }

        public void SendTrade(Trade trade, Description description, string clientID)
        {
            CallClientMethod(clientID, client => client.Algorithm.OnTrade(trade, description));
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
            var configuringClient = new ConfiguringClient { ClientID = clientID, Algorithm = algorithm, Instruments = instrumentID };
            if (!_savedClients.TryAdd(clientID, configuringClient))
            {
                throw new InvalidOperationException("Another client exists with ID " + clientID + ".");
            }
        }

        private void InitializeClient(string clientID, IAlgorithm algorithm, params string[] instrumentID)
        {
            ClientHandler.OnClientConnect(clientID, algorithm, this);
            instrumentID.ToList().ForEach(instrument => ClientHandler.OnSubscribe(instrument, true, clientID));

            CallClientMethod(clientID, client => client.Algorithm.OnLoad(new LocalClientOperator(clientID, ClientHandler)));
        }

        private void CallClientMethod(string clientID, Action<Client> action)
        {
            if (ClientHandler.Clients.TryGetValue(clientID, out var client))
            {
                try
                {
                    action(client);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("{0}, {1}\n{2}", DateTime.Now, ex.Message, ex.StackTrace);
                }
            }
            else
            {
                throw new InvalidOperationException("No such client with ID " + clientID + ".");
            }
        }
    }
}