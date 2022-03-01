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

namespace Evelyn.Internal
{
    internal class EngineClientHandler : IClientHandler
    {
        private readonly IDictionary<string, Client> _clients = new Dictionary<string, Client>();
        private EngineBroker? _broker;
        private EngineFeedSource? _feedSource;
        private EngineFeedHandler? _feedHandler;

        internal EngineBroker Broker => _broker ?? throw new NoValueException("Engine broker has no value.");

        internal EngineFeedSource FeedSource => _feedSource ?? throw new NoValueException("Enginefeed source has no value.");

        internal EngineFeedHandler FeedHandler => _feedHandler ?? throw new NoValueException("Engine feed handler has no value.");

        public List<Client> Clients => new List<Client>(_clients.Values);

        internal Client this[string clientID]
        {
            get => _clients[clientID];
        }

        public void OnClientConnect(string clientID, IClientService service)
        {
            if (_clients.ContainsKey(clientID))
            {
                throw new DuplicatedClientException("Another client exists with ID " + clientID + ".");
            }

            _clients.Add(clientID, new Client(service, clientID));
        }

        internal void OnClientConnect(string clientID, IAlgorithm algorithm, IClientService service)
        {
            if (_clients.ContainsKey(clientID))
            {
                throw new DuplicatedClientException("Another client exists with ID " + clientID + ".");
            }

            _clients.Add(clientID, new Client(service, clientID, algorithm));
        }

        public void OnClientDisconnect(string clientID)
        {
            if (!_clients.ContainsKey(clientID))
            {
                throw new NoSuchClientException("No such clien with ID " + clientID + ".");
            }

            FeedSource.Subscribe(_clients[clientID].Subscription.Instruments, false);
            _clients.Remove(clientID);
        }

        public void OnDeleteOrder(string orderID, string clientID)
        {
            throw new NotImplementedException();
        }

        public void OnNewOrder(NewOrder newOrder, string clientID)
        {
            throw new NotImplementedException();
        }

        public void OnSubscribe(string instrumentID, bool isSubscribed, string clientID)
        {
            if (!_clients.ContainsKey(clientID))
            {
                throw new NoSuchClientException("No such clien with ID " + clientID + ".");
            }

            var client = _clients[clientID];
            var currentInstruments = client.Subscription.Instruments;

            /*
             * Response to subscription of the instrument shall be routed to this client.
             */
            client.Subscription.MarkSubscriptionResponse(instrumentID, waitResponse: true);

            if (isSubscribed)
            {
                if (currentInstruments.Contains(instrumentID))
                {
                    /*
                     * If instrument has been subscribed, nothing happens, and sends a response with error.
                     */
                    FeedHandler.OnSubscribed(instrumentID, new Description { Code = 101, Message = "Duplicated subscription for " + instrumentID + "." }, true);
                }
                else
                {
                    FeedSource.Subscribe(new List<string> { instrumentID }, true);
                    client.Subscription.Subscribe(instrumentID, true);
                }
            }
            else
            {
                if (!currentInstruments.Contains(instrumentID))
                {
                    /*
                     * If instrument has been unsubscribed or never subscribed, nothing happens, and sends a response with error.
                     */
                    FeedHandler.OnSubscribed(instrumentID, new Description { Code = 102, Message = " No such subscription for " + instrumentID + "." }, false);
                }
                else
                {
                    FeedSource.Subscribe(new List<string> { instrumentID }, false);
                    client.Subscription.Subscribe(instrumentID, false);
                }
            }
        }

        internal void Configure(EngineBroker broker, EngineFeedSource feedSource, EngineFeedHandler feedHandler)
        {
            _broker = broker;
            _feedSource = feedSource;
            _feedHandler = feedHandler;
        }
    }
}