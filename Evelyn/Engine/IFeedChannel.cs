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
namespace PetriSoft.Evelyn.Engine
{
    /// <summary>
    /// Feed channel.
    /// </summary>
    /// <typeparam name="T">Feed type.</typeparam>
    public interface IFeedChannel<T> where T : MarketItem
    {
        /// <summary>
        /// Send feed item to all listeners on this channel.
        /// </summary>
        /// <param name="item">Feed item.</param>
        public void Send(T item);

        /// <summary>
        /// Set listeners on this channel.
        /// </summary>
        /// <param name="actions">Listening actions.</param>
        /// <returns></returns>
        public IFeedChannel<T> Listening(params Action<T>[] actions);
    }
}
