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
    internal class LocalClientOperator : IOperator
    {
        private readonly string _clientID;
        private readonly EngineClientHandler _clientHandler;

        public LocalClientOperator(string clientID, EngineClientHandler clientHandler)
        {
            _clientID = clientID;
            _clientHandler = clientHandler;
        }

        public DateOnly TradingDay => _clientHandler.Broker.TradingDay;

        public ILogger Logger => _clientHandler.Clients[_clientID].Logger;

        public void Delete(DeleteOrder deleteOrder, OrderOption? option = null)
        {
            var optionOrDefault = option ?? new OrderOption();

            switch (option?.Trigger.When)
            {
                case TriggerType.Time:
                case TriggerType.StateChange:

                    _clientHandler.FeedHandler.ScheduleJob("Delete:" + deleteOrder.OrderID, () => _clientHandler.OnDeleteOrder(deleteOrder, _clientID), deleteOrder.InstrumentID, optionOrDefault);
                    break;

                case TriggerType.Immediate:
                default:

                    _clientHandler.OnDeleteOrder(deleteOrder, _clientID);
                    break;
            }
        }

        public void New(NewOrder newOrder, OrderOption? option = null)
        {
            var optionOrDefault = option ?? new OrderOption();

            switch (option?.Trigger.When)
            {
                case TriggerType.Time:
                case TriggerType.StateChange:

                    _clientHandler.FeedHandler.ScheduleJob("New:" + newOrder.OrderID, () => _clientHandler.OnNewOrder(newOrder, _clientID), newOrder.InstrumentID, optionOrDefault);
                    break;

                case TriggerType.Immediate:
                default:

                    _clientHandler.OnNewOrder(newOrder, _clientID);
                    break;
            }
        }
    }
}
