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
using PetriSoft.Evelyn.Model;

namespace PetriSoft.Evelyn.Plugin
{
    public interface IFeedHandler
    {
        /// <summary>
        /// Tick feed callback and this method must be implemented.
        /// </summary>
        /// <param name="tick">Tick feed.</param>
        public void OnFeed(Tick tick);

        /// <summary>
        /// OHLC feed callback, and the method is optional because not all data source provide this kind of feed.
        /// <para>OHLC feed can be input via <see cref="Engine.IMultiChannel"/> and the callback receives the feeds.</para>
        /// </summary>
        /// <param name="ohlc">OHLC feed.</param>
        public void OnFeed(OHLC ohlc);

        /// <summary>
        /// Instrument event callback for state changes of the instrument within a trading day.
        /// </summary>
        /// <param name="instrument">Instrument event feed.</param>
        public void OnInstrument(Instrument instrument);

        /// <summary>
        /// Data feed subscription callback.
        /// </summary>
        /// <param name="instrumentID">Instrument ID.</param>
        /// <param name="description">Description of the feed containing error code and message if it has.</param>
        public void OnSubscribed(string instrumentID, Description description);

        /// <summary>
        /// Data feed un-subscription callback.
        /// </summary>
        /// <param name="instrumentID">Instrument ID.</param>
        /// <param name="description">Description of the feed containing error code and message if it has.</param>
        public void OnUnsubscribed(string instrumentID, Description description);
    }
}