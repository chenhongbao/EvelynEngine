﻿/*
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
        /// Add interceptor to process the <see cref="Trade"/> type before forwarding to client.
        /// </summary>
        /// <param name="interceptor">Interceptor delegate.</param>
        /// <returns></returns>
        public IEngine AddInterceptor(Action<Trade, IDataChannel> interceptor);

        /// <summary>
        /// Add interceptor to process the <see cref="NewOrder"/> type of object before forwarding to client.
        /// </summary>
        /// <param name="interceptor">Interceptor delegate.</param>
        /// <returns></returns>
        public IEngine AddInterceptor(Action<NewOrder, IDataChannel> interceptor);

        /// <summary>
        /// Add interceptor to process the <see cref="Tick"/> type of object before forwarding to client.
        /// </summary>
        /// <param name="interceptor">Interceptor delegate.</param>
        /// <returns></returns>
        public IEngine AddInterceptor(Action<Tick, IDataChannel> interceptor);

        /// <summary>
        /// Add interceptor to process the <see cref="OHLC"/> type of object before forwarding to client.
        /// </summary>
        /// <param name="interceptor">Interceptor delegate.</param>
        /// <returns></returns>
        public IEngine AddInterceptor(Action<OHLC, IDataChannel> interceptor);

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
        /// Enable management console. Local access to management service is open by default.
        /// </summary>
        /// <param name="remote"><c>true</c> to enable management access from remote machine.</param>
        /// <param name="managementListening">Listening port for remote management access.</param>
        /// <returns></returns>
        public IEngine EnableManagement(bool remote = false, EndPoint? managementListening = null);

        /// <summary>
        /// Get client service listening end point, or <c>null</c> if it is not listening for client connection.
        /// </summary>
        public EndPoint? ClientServiceEndPoint { get; }

        /// <summary>
        /// Get management end point, or <c>null</c> if it not listening for management request.
        /// </summary>
        public EndPoint? ManagementEndPoint { get; }

        /// <summary>
        /// Get engine broker if it is intialized.
        /// </summary>
        public IEngineBroker EngineBroker { get; }

        /// <summary>
        /// Get configurator if it is set up.
        /// </summary>
        public IConfigurator Configurator { get; }
    }
}
