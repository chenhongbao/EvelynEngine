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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PetriSoft.Evelyn.Engine
{
    public class Engine : IEngine
    {
        private static readonly int _CLIENT_LISTEN_PORT = 10990;
        private static readonly int _MANAGE_LISTEN_PORT = 10992;

        private EndPoint? _cliSvcEP = null;
        private EndPoint? _mngEP = null;

        public void Setup(IConfigurator configurator)
        {
            throw new NotImplementedException();
        }

        public IEngine EnableLocalClient(params LocalClient[] clients)
        {
            throw new NotImplementedException();
        }

        public IEngine EnableRemoteClient(EndPoint? point = null, IEndPointService? listening = null)
        {
            throw new NotImplementedException();
        }

        public IEngine EnableManagement(bool local, bool remote = false, EndPoint? managementListening = null)
        {
            throw new NotImplementedException();
        }

        public EndPoint? ClientServiceEndPoint => _cliSvcEP;

        public EndPoint? ManagementEndPoint => _mngEP;

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

            throw new ResourceInavailableException("Socket failed listening at port " + port + ".");
        }
    }
}
