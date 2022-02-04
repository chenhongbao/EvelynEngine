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
using System.Net;

namespace PetriSoft.Evelyn.Engine
{
    /// <summary>
    /// Remove client acceptor.
    /// </summary>
    public interface IEndPointService
    {
        /// <summary>
        /// Set the end point service to listen at port.
        /// </summary>
        /// <param name="listening">Listening end point.</param>
        /// <returns>This end point service.</returns>
        public IEndPointService ListenAt(EndPoint listening);

        /// <summary>
        /// Accept remote connection. A end point service can have multiple remote channel acceptors
        /// that will receive callback on every incomming connection.
        /// </summary>
        /// <typeparam name="T">Channeling data type.</typeparam>
        /// <param name="acceptor">Acceptor delegate.</param>
        /// <returns>This end point service.</returns>
        public IEndPointService Accept<T>(Action<IRemoteChannel> acceptor);
    }
}