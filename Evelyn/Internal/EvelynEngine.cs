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
        private readonly EngineOrderHandler _orderHandler;
        private readonly EngineFeedSource _feedSource;
        private readonly EngineFeedHandler _feedHandler;
        private readonly EngineClientHandler _clientHandler;
        private readonly ISet<IClientService> _registeredServices = new HashSet<IClientService>();

        private IConfigurator? _configurator;

        internal EvelynEngine()
        {
            _localService = new LocalClientService();
            _clientHandler = new EngineClientHandler();
            _orderHandler = new EngineOrderHandler(_clientHandler);
            _feedHandler = new EngineFeedHandler(_clientHandler);
            _broker = new EngineBroker(_orderHandler);
            _feedSource = new EngineFeedSource(_feedHandler);

        }

        private LocalClientService LocalService => _localService;

        public EndPoint? ClientServiceEndPoint => throw new NotImplementedException();

        public EndPoint? ManagementEndPoint => throw new NotImplementedException();

        public IConfigurator Configurator => _configurator ?? throw new NoValueException("Engine is not configured yet.");

        public IEvelyn AlterLocalClient(string clientID, params string[] instrumentID)
        {
            var client = _clientHandler[clientID];

            /*
             * Call IClientHandler methods to handle the instruments, so all kinds of subscription go through the same logic.
             * For every new subscribed instrument, susbcribe it, and for those not in the altered instruments' list, unsubscribe.
             */
            instrumentID.ToList().ForEach(instrument => _clientHandler.OnSubscribe(instrument, true, clientID));
            client.Subscription.Instruments
                .Where(instrument => !instrumentID.Contains(instrument)).ToList()
                .ForEach(instrument => _clientHandler.OnSubscribe(instrument, false, clientID));
            return this;
        }

        public void Configure(IConfigurator configurator)
        {
            _configurator = configurator;
            Configurator.Create(out IBroker broker, out IFeedSource feedSource);

            _feedSource.Configure(feedSource);
            _broker.Configure(broker);
            _clientHandler.Configure(_broker, _feedSource, _feedHandler);

            LocalService.Configure(_clientHandler);
            _registeredServices.ToList().ForEach(service => service.Configure(_clientHandler));
        }

        public IEvelyn RegisterLocalClient(string clientID, IAlgorithm algorithm, params string[] instrumentID)
        {
            LocalService.RegisterClient(clientID, algorithm, instrumentID);
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

        public IEvelyn EnableRemoteManagement()
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableRemoteManagement(EndPoint listeningEndPoint)
        {
            throw new NotImplementedException();
        }

        public IEvelyn GenerateOHLC()
        {
            _feedHandler.RegisterOHLCGenerator(new OHLCGenerator());
            return this;
        }

        public IEvelyn GenerateOHLC(IOHLCGenerator generator)
        {
            _feedHandler.RegisterOHLCGenerator(generator);
            return this;
        }

        public IEvelyn RegisterClientService()
        {
            throw new NotImplementedException();
        }

        public IEvelyn RegisterClientService(EndPoint listeningEndPoint)
        {
            throw new NotImplementedException();
        }

        public IEvelyn RegisterClientService(IClientService clientService)
        {
            _registeredServices.Add(clientService);
            return this;
        }

        public IEvelyn RegisterInstrument(params Instrument[] instruments)
        {
            instruments.ToList().ForEach(instrument => _feedHandler.SaveInstrument(instrument));
            return this;
        }
    }
}