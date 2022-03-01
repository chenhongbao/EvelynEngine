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

namespace Evelyn
{
    public interface IEvelyn
    {
        public static IEvelyn New()
        {
            return new Internal.EvelynEngine();
        }

        public void Configure(IConfigurator configurator);

        public IEvelyn EnableOHLC();

        public IEvelyn EnableOHLC(IOHLCGenerator generator);

        public IEvelyn EnableRemoteClient();

        public IEvelyn EnableRemoteClient(EndPoint listeningEndPoint);

        public IEvelyn EnableRemoteClient(IClientService clientService);

        public IEvelyn EnableLocalClient(string clientID, IAlgorithm algorithm, params string[] instrumentID);

        /// <summary>
        /// Alter instrument subscription for the client with the given name. If an instrument
        /// in the given instruments' array hasn't been subscribed, it is subscribed. And if a
        /// previous subscribed instrument doesn't exist in the given instruments' array, it is
        /// unsubscribed.
        /// </summary>
        /// <param name="clientID">Client ID.</param>
        /// <param name="instrumentID">Subscribed instruments for client.</param>
        /// <returns><see cref="IEvelyn"/> instance.</returns>
        public IEvelyn AlterLocalClient(string clientID, params string[] instrumentID);

        public IEvelyn EnableManagement();

        public IEvelyn EnableLocalManagement();

        public IEvelyn EnableRemoteManagement();

        public IEvelyn EnableRemoteManagement(EndPoint listeningEndPoint);

        public IEvelyn InitializeInstrument(params Instrument[] instruments);

        public EndPoint? ClientServiceEndPoint { get; }

        public EndPoint? ManagementEndPoint { get; }

        public IConfigurator Configurator { get; }
    }
}
