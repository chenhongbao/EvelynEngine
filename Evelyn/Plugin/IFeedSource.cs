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

namespace Evelyn.Plugin
{
    public interface IFeedSource
    {
        /// <summary>
        /// Subscribe for an instrument and receive market via the given <see cref="IFeedHandler"/>. 
        /// An instrument can be subscribed with multiple <see cref="IFeedHandler"/>s and they all
        /// will be called on the incoming market data.
        /// </summary>
        /// <param name="instrumentID">Instrument ID</param>
        /// <param name="feedHandler">Market data handler.</param>
        public void Subscribe(string instrumentID, IFeedHandler feedHandler);

        public void Unsubscribe(string instrumentID);

        public void Unsubscribe(string instrumentID, IFeedHandler feedHandler);
    }
}
