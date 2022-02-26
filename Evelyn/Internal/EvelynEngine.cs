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
using PetriSoft.Evelyn.Plugin;
using System.Net;

namespace PetriSoft.Evelyn.Internal
{
    internal class EvelynEngine : IEvelyn
    {
        public EndPoint? ClientServiceEndPoint => throw new NotImplementedException();

        public EndPoint? ManagementEndPoint => throw new NotImplementedException();

        public IConfigurator Configurator => throw new NotImplementedException();

        public IEvelyn EnableLocalClient(LocalClient client)
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableManagement(bool remote = false, EndPoint? managementListening = null)
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableOHLC(IOHLCGenerator? generator = null)
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableRemoteClient(EndPoint? point = null)
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableRemoteClient(IClientService clientService)
        {
            throw new NotImplementedException();
        }

        public void Setup(IConfigurator configurator)
        {
            throw new NotImplementedException();
        }
    }
}