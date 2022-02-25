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
using System.Net;

namespace PetriSoft.Evelyn.Engine
{
    /// <summary>
    /// Remote client service.
    /// </summary>
    public interface IClientService
    {
        /// <summary>
        /// Send <see cref="Tick"/> to client and client calls <see cref="Plugin.IFeedHandler.OnFeed(Tick)"/>.
        /// </summary>
        /// <param name="tick">Tick data.</param>
        /// <param name="clientID">Client to receive the data.</param>
        public void ToClient(Tick tick, string clientID);

        /// <summary>
        /// Send <see cref="OHLC"/> to client and client calls <see cref="Plugin.IFeedHandler.OnFeed(OHLC)"/>.
        /// </summary>
        /// <param name="ohlc">OHCL data.</param>
        ///  <param name="clientID">Client to receive the data.</param>
        public void ToClient(OHLC ohlc, string clientID);

        /// <summary>
        /// Send <see cref="Instrument"/> to client and client calls <see cref="Plugin.IFeedHandler.OnInstrument(Instrument)"/>.
        /// </summary>
        /// <param name="instrument">Instrument data.</param>
        /// <param name="clientID">Client to receive the data.</param>
        public void ToClient(Instrument instrument, string clientID);

        /// <summary>
        /// Send subscrption feedback to client and client calls <see cref="Plugin.IFeedHandler.OnSubscribed(string, Description, bool)"/>.
        /// </summary>
        /// <param name="instrumentID">Instrument ID.</param>
        /// <param name="description">Description of operation feedback.</param>
        /// <param name="subscribed"><c>True</c> if instrument is subscribed, <c>false</c> otherwise.</param>
        /// <param name="clientID">Client to receive the data.</param>
        public void ToClient(string instrumentID, Description description, bool subscribed, string clientID);

        /// <summary>
        /// Send trade feedback to client and client calls <see cref="Plugin.IOrderHandler.OnTrade(Trade, Description)"/>.
        /// </summary>
        /// <param name="trade">Trade data.</param>
        /// <param name="description">Description of the trade feedback.</param>
        /// <param name="clientID">Client to receive the data.</param>
        public void ToClient(Trade trade, Description description, string clientID);

        /// <summary>
        /// Add order handler.
        /// </summary>
        /// <param name="orderAcceptor">Order acceptor.</param>
        public void FromClient(IOrderAcceptor orderAcceptor);

        /// <summary>
        /// Add subscription acceptor.
        /// </summary>
        /// <param name="subscriptionAcceptor">Subscription acceptor.</param>
        public void FromClient(ISubscriptionAcceptor subscriptionAcceptor);
    }
}