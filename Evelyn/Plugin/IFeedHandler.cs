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
    public interface IFeedHandler
    {
        public void OnFeed(Tick tick);

        public void OnFeed(OHLC ohlc);

        public void OnInstrument(Instrument instrument);

        /// <summary>
        /// <para>
        /// Subscription callback notifying the instrument susbcription status. If the
        /// feed source is disconnected or log off from exchange, it receives no more 
        /// market data, or it needs to (re)subscribe again for the same instrument to
        /// get market data once it is reconnected or logon.
        /// </para>
        /// <para>
        /// When the above situation happens, this method must be called with <c>false</c>
        /// marking there is no more market data for the specified instrument and the
        /// subscription request must be sent again.
        /// </para>
        /// </summary>
        /// <param name="instrumentID">Instrument ID.</param>
        /// <param name="description">Description.</param>
        /// <param name="subscribed"><c>true</c> if the instrument is subscribed, <c>false</c> otherwise.</param>
        public void OnSubscribed(string instrumentID, Description description, bool subscribed);
    }
}