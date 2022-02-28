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
        public EndPoint? ClientServiceEndPoint => throw new NotImplementedException();

        public EndPoint? ManagementEndPoint => throw new NotImplementedException();

        public IConfigurator Configurator => throw new NotImplementedException();

        public IEvelyn AlterLocalClient(string name, params string[] instrumentID)
        {
            throw new NotImplementedException();
        }

        public IEvelyn AlterLocalClient(IAlgorithm algorithm, params string[] instrumentID)
        {
            throw new NotImplementedException();
        }

        public void Configure(IConfigurator configurator)
        {
            throw new NotImplementedException();
        }

        public IEvelyn EnableLocalClient(string name, IAlgorithm algorithm, params string[] instrumentID)
        {
            throw new NotImplementedException();
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

        public IEvelyn RegisterInstrument(params Instrument[] instruments)
        {
            throw new NotImplementedException();
        }
    }
}