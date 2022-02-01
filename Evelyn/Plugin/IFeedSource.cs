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
namespace PetriSoft.Evelyn.Plugin
{
    public interface IFeedSource
    {
        /// <summary>
        /// Subscribe data feed of the specified instrument.
        /// </summary>
        /// <param name="instrumentID">Instrument ID to subscribe.</param>
        /// <param name="feedHandler">Data feed handler for the subscribed instrument.</param>
        public void Subscribe(string instrumentID, IFeedHandler feedHandler);

        /// <summary>
        /// Unsubscribe the data feed of the specified instrument.
        /// </summary>
        /// <param name="instrumentID">Instrument ID to unsubscribe.</param>
        public void Unsubscribe(string instrumentID);
    }
}
