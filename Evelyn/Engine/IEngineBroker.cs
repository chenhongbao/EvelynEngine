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

namespace PetriSoft.Evelyn.Engine
{
    /// <summary>
    /// Broker for clients' requests inside engine.
    /// </summary>
    public interface IEngineBroker
    {
        /// <summary>
        /// Register client connection.
        /// </summary>
        /// <param name="channel">Remote connection from clients.</param>
        public void RegisterClientChannel(IRemoteChannel channel);

        /// <summary>
        /// Register local clients.
        /// </summary>
        /// <param name="client">Local client.</param>
        public void RegisterClient(LocalClient client);

        /// <summary>
        /// Intercept the <see cref="Trade"/> type data.
        /// </summary>
        /// <param name="interceptor">Interceptor to handle <see cref="Trade"/>.</param>
        public void AddInterceptpr(Action<Trade, IDataChannel> interceptor);

        /// <summary>
        /// Intercept the <see cref="NewOrder"/> type data.
        /// </summary>
        /// <param name="interceptor">Interceptor to handle <see cref="NewOrder"/>.</param>
        public void AddInterceptpr(Action<NewOrder, IDataChannel> interceptor);

        /// <summary>
        /// Intercept the <see cref="Tick"/> type data.
        /// </summary>
        /// <param name="interceptor">Interceptor to handle <see cref="Tick"/>.</param>
        public void AddInterceptpr(Action<Tick, IDataChannel> interceptor);

        /// <summary>
        /// Intercept the <see cref="OHLC"/> type data.
        /// </summary>
        /// <param name="interceptor">Interceptor to handle <see cref="OHLC"/>.</param>
        public void AddInterceptpr(Action<OHLC, IDataChannel> interceptor);
    }
}