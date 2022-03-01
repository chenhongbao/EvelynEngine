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
    internal class EvelynEngine : IEvelyn
    {
        private readonly LocalClientService _localService;
        private readonly EngineBroker _broker;
        private readonly EngineFeedSource _feedSource;
        private readonly EngineFeedHandler _feedHandler;
        private readonly EngineClientHandler _clientHandler;
        private IConfigurator? _configurator;

        public EvelynEngine()
        {
            _localService = new LocalClientService(); 
            _clientHandler = new EngineClientHandler();
            _feedHandler = new EngineFeedHandler(_clientHandler);
            _broker = new EngineBroker();
            _feedSource = new EngineFeedSource(_feedHandler);
            
        }

        private LocalClientService LocalService => _localService;

        private EngineFeedSource FeedSource => _feedSource.IsConfigured ? _feedSource : throw new NoValueException("Feed source is not initialized yet.");

        private EngineBroker Broker => _broker.IsConfigured ? _broker : throw new NoValueException("Engine broker is not initialized yet.");

        public EndPoint? ClientServiceEndPoint => throw new NotImplementedException();

        public EndPoint? ManagementEndPoint => throw new NotImplementedException();

        public IConfigurator Configurator => _configurator ?? throw new NoValueException("Engine is not configured yet.");

        public IEvelyn AlterLocalClient(string clientID, params string[] instrumentID)
        {
            /* 
             * Find out the new added and removed instruments.
             */
            _clientHandler[clientID].Subscription.AlterInstruments(instrumentID, out IEnumerable<string> added, out IEnumerable<string> removed);

            /*
             * Call IClientHandler methods to handle the instruments, so all kinds of subscription go through the same logic.
             */
            added.ToList().ForEach(instrument => _clientHandler.OnSubscribe(instrument, true, clientID));
            removed.ToList().ForEach(instrument => _clientHandler.OnSubscribe(instrument, false, clientID));
            return this;
        }

        public void Configure(IConfigurator configurator)
        {
            _configurator = configurator;
            Configurator.Create(out IBroker broker, out IFeedSource feedSource);

            _feedSource.Configure(feedSource);
            _broker.Configure(broker);
            _clientHandler.Configure(_broker, _feedSource, _feedHandler);

            LocalService.Service(_clientHandler);
        }

        public IEvelyn EnableLocalClient(string clientID, IAlgorithm algorithm, params string[] instrumentID)
        {
            LocalService.EnableClient(clientID, algorithm, instrumentID);
            return this;
        }

        public IEvelyn EnableLocalManagement()
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableManagement()
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableOHLC()
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableOHLC(IOHLCGenerator generator)
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableRemoteClient()
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableRemoteClient(EndPoint listeningEndPoint)
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableRemoteClient(IClientService clientService)
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableRemoteManagement()
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableRemoteManagement(EndPoint listeningEndPoint)
        {
            throw new NotImplementedException();
        }

        public IEvelyn InitializeInstrument(params Instrument[] instruments)
        {
            throw new NotImplementedException();
        }
    }
}