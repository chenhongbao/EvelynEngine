/*
Null value exception for Evelyn Engine, a quantitative trading engine by Chen Hongbao.
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
using PetriSoft.Evelyn.Plugin;

using System.Net;
using System.Net.Sockets;

namespace PetriSoft.Evelyn.Engine
{
    internal class Engine : IEngine
    {
        private static readonly int _CLIENT_LISTEN_PORT = 10990;
        private static readonly int _MANAGE_LISTEN_PORT = 10992;

        private EndPoint? _cliSvcEP = null;
        private EndPoint? _mngEP = null;

        private IEndPointService? _cliEpSvc = null;
        private IEndPointService? _mngSvc = null;
        private IConfigurator? _configurator = null;
        private IEngineBroker? _engineBroker = null;

        public void Setup(IConfigurator configurator)
        {
            _configurator = configurator ?? throw new ArgumentNullException("Configurator is null.");
            _configurator.Create(out var broker, out var feedSource);
            _engineBroker = new EngineBroker(broker, feedSource);
        }

        public IEngine EnableLocalClient(params LocalClient[] clients)
        {
            foreach (var client in clients)
            {
                (_engineBroker ?? throw new NullValueException("Broker engine is not initialized.")).RegisterClient(client);
            }
            return this;
        }

        public IEngine EnableRemoteClient(EndPoint? point = null, IEndPointService? listening = null)
        {
            if (_cliSvcEP != null)
            {
                throw new InvalidOperationException("Engine has been listening for client connection at " + _cliSvcEP.ToString() + ".");
            }

            _cliSvcEP = point ?? SelectProperServerEndPoint(_CLIENT_LISTEN_PORT);

            _cliEpSvc = listening ?? new DefaultClientService();
            _cliEpSvc.ListenAt(_cliSvcEP);
            _cliEpSvc.Accept(RemoteChannelAcceptor);

            return this;
        }

        public IEngine EnableManagement(bool remote = false, EndPoint? managementListening = null)
        {
            _mngSvc = new ManagementService(this);
            if (remote)
            {
                _mngEP = managementListening ?? SelectProperServerEndPoint(_MANAGE_LISTEN_PORT);
                _mngSvc.ListenAt(_mngEP);
            }
            return this;
        }

        public EndPoint? ClientServiceEndPoint => _cliSvcEP;

        public EndPoint? ManagementEndPoint => _mngEP;

        public IEngineBroker EngineBroker => _engineBroker ?? throw new NullValueException("Engine broker is null.");

        public IConfigurator Configurator => _configurator ?? throw new NullValueException("Configurator is null.");

        private void RemoteChannelAcceptor(IRemoteChannel channel)
        {
            (_engineBroker ?? throw new NullValueException("Engine broker is not initialized.")).RegisterClientChannel(channel);
        }

        private EndPoint SelectProperServerEndPoint(int port)
        {
            foreach (var address in Dns.GetHostAddresses(IPAddress.Any.ToString()))
            {
                IPEndPoint ep = new IPEndPoint(address, port);
                try
                {
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(ep);
                    socket.Close();
                    return ep;
                }
                catch
                {
                }
            }

            throw new ResourceUnavailableException("Socket failed listening at port " + port + ".");
        }
    }
}
