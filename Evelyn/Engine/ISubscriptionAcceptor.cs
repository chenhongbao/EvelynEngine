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
namespace PetriSoft.Evelyn.Engine
{
    /// <summary>
    /// Subscription acceptor.
    /// </summary>
    public interface ISubscriptionAcceptor
    {
        /// <summary>
        /// Accept an subscription request.
        /// </summary>
        /// <param name="instrumentID">Instrument to subscribe.</param>
        /// <param name="clientID">Client sending the request.</param>
        public void OnSubscription(string instrumentID, string clientID);

        /// <summary>
        /// Accept an unsubscription request.
        /// </summary>
        /// <param name="instrumentID">Instrument to unsubscribe.</param>
        /// <param name="clientID">Client sending the request.</param>
        public void OnUnsubscription(string instrumentID, string clientID);
    }
}