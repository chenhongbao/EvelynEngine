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
using Evelyn.Internal.Logging;
using Evelyn.Model;
using Evelyn.Plugin;
using Microsoft.Extensions.Logging;

namespace Evelyn.Internal
{
    internal class EngineOrderHandler : IOrderHandler
    {
        private readonly EngineClientHandler _clients;

        private ILogger Logger { get; init; } = Loggers.CreateLogger(nameof(EngineOrderHandler));

        internal EngineOrderHandler(EngineClientHandler clientHandler)
        {
            _clients = clientHandler;
        }

        public void OnTrade(Trade trade, Description description)
        {
            foreach (var client in _clients.Clients)
            {
                foreach (var order in client.Orders)
                {
                    if (order.BrokerOrderID == trade.OrderID)
                    {
                        /*
                         * Rewrite the order ID back to original ID.
                         */
                        trade.OrderID = order.OriginalOrder.OrderID;
                        client.Service.SendTrade(trade, description, client.ClientID);
                        return;
                    }
                }
            }

            Logger.LogWarning("{0}\n{1}", "No such order with broker's order ID " + trade.OrderID, new System.Diagnostics.StackTrace().ToString());
        }
    }
}