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
using Evelyn.Plugin;
using Microsoft.Extensions.Logging;

namespace Evelyn.Internal
{
    internal class EngineOrderHandler : IOrderHandler
    {
        private readonly EngineClientHandler _clientHandler;

        private ILogger? _logger;

        private ILogger Logger => _logger ?? throw new NullReferenceException("Logger has no value.");

        internal EngineOrderHandler(EngineClientHandler clientHandler)
        {
            _clientHandler = clientHandler;
        }

        internal void Configure(ILogger logger)
        {
            _logger = logger;
        }

        public void OnTrade(Trade trade, Description description)
        {
            if (description.Code != ErrorCodes.OK)
            {
                Logger.LogError("Trade error {0}/{1}.", description.Code, description.Message);
            }

            foreach (var client in _clientHandler.Clients.Values)
            {
                foreach (var order in client.Orders.Values)
                {
                    if (order.BrokerOrderID == trade.OrderID)
                    {
                        /*
                         * Rewrite the order ID back to original ID.
                         */
                        trade.OrderID = order.OriginalOrder.OrderID;

                        order.Status = trade.Status;
                        order.Trades.Enqueue(trade);

                        client.Service.SendTrade(trade, description, client.ClientID);
                        return;
                    }
                }
            }

            /*
             * For a deleted or completed order, return error response, and the order ID
             * is an empty string. In this case, no such order found.
             */
            Logger.LogWarning("No such order with broker's order ID \'{0}\'.\n{1}", trade.OrderID, new System.Diagnostics.StackTrace().ToString());
        }
    }
}