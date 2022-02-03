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

namespace PetriSoft.Evelyn.Engine
{
    /// <summary>
    /// Evelyn engine interface.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Set up the engine.
        /// </summary>
        /// <param name="configurator">Customized configurator for the underlying broker and data feed source.</param>
        public void Setup(IConfigurator configurator);

        /// <summary>
        /// Enable server for remote client.
        /// </summary>
        /// <param name="point">Listening end point, <c>null</c> for default listening end point.</param>
        /// <param name="listening">Server listening service providing communicating protocol, <c>null</c> for default protocol.</param>
        /// <returns>This <see cref="IEngine"/> object.</returns>
        public IEngine EnableRemoteClient(EndPoint? point = null, IEndPointService? listening = null);

        /// <summary>
        /// Enable support for local clients.
        /// </summary>
        /// <param name="clients">Clients to run localy in this engine.</param>
        /// <returns>This <see cref="IEngine"/> object.</returns>
        public IEngine EnableLocalClient(params LocalClient[] clients);

        /// <summary>
        /// Enable management console.
        /// </summary>
        /// <param name="local"><c>true</c> to enable management access from local machine.</param>
        /// <param name="remote"><c>true</c> to enable management access from remote machine.</param>
        /// <param name="managementListening">Listening port for remote management access.</param>
        /// <returns></returns>
        public IEngine EnableManagement(bool local, bool remote = false, EndPoint? managementListening = null);
    }
}
