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

namespace Evelyn
{
    public interface IEvelyn
    {
        public static IEvelyn NewInstance => new Internal.EvelynEngine();

        public IConfigurator Configurator { get; }

        public IEvelyn DeregisterClient(string clientID);
        public IEvelyn Configure(IConfigurator configurator);
        public IEvelyn GenerateOHLC(IOHLCGenerator generator);
        public IEvelyn RegisterLoggerProvider(ILoggerProvider provider);
        public IEvelyn RegisterRemoteClient(IClientService clientService);
        public IEvelyn RegisterManagement(IManagementService managementService);

        public IEvelyn RegisterLocalClient(string clientID, IAlgorithm algorithm, params string[] instrumentID);

        /// <summary>
        /// Change instrument subscription for the specified client with the client ID.
        /// <para>
        /// <list type="bullet">
        /// <item>If an instrument has been subscribed and is in the given instruments' array, no more subscription for the same instrument.</item>
        /// <item>If an instrument has been subscribed but not in the given instruments' array, the instrument is <b>un</b>subscribed for the client.</item>
        /// <item>If an instrument hasn't been subscribed but in the given instruments' array, the instrument is subscribed for the client.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="clientID">Client ID.</param>
        /// <param name="instrumentID">Subscribed instruments for client.</param>
        /// <returns><see cref="IEvelyn"/> instance.</returns>
        public IEvelyn AlterClient(string clientID, params string[] instrumentID);

        /// <summary>
        /// Registered instruments are sent to clients when clients are being loaded.
        /// </summary>
        /// <param name="instruments">Registered instruments.</param>
        /// <returns><see cref="IEvelyn"/> instance.</returns>
        public IEvelyn RegisterInstrument(params Instrument[] instruments);
    }
}
