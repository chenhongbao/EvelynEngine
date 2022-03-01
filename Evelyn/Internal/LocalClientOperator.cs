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
using System.Timers;

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

        public void Delete(string orderID, OrderOption? option = null)
        {
            if (!TryFindInstrumentIDByOrderID(orderID, out string instrumentID))
            {
                /*
                 * No such order, send a empty trade with error description.
                 */
                _clientHandler[_clientID].Service.SendTrade(
                    new Trade
                    {
                        InstrumentID = String.Empty,
                        TradingDay = DateOnly.MaxValue,
                        TimeStamp = DateTime.MaxValue,
                        OrderID = orderID,
                        Price = double.MaxValue,
                        Quantity = int.MaxValue,
                        Direction = default(Direction),
                        Offset = default(Offset),
                        TradeID = String.Empty,
                        TradePrice = double.MaxValue,
                        TradeQuantity = int.MaxValue,
                        LeaveQuantity = int.MaxValue,
                        TradeTimeStamp = DateTime.MaxValue,
                        Status = OrderStatus.Deleted,
                        Message = "No such order."
                    },
                    new Description
                    {
                        Code = 1,
                        Message = "No such order with ID " + orderID + "."
                    },
                    _clientID);
            }
            else
            {
                switch (option?.Trigger.When)
                {
                    case TriggerType.Moment:

                        var moment = option?.Trigger.Moment ?? DateTime.Now;
                        ScheduleOrderByMoment(() => _clientHandler.OnDeleteOrder(orderID, _clientID), moment);
                        break;

                    case TriggerType.StateChange:

                        var state = option?.Trigger.StateChange ?? InstrumentState.Continous;
                        _clientHandler.FeedHandler.ScheduleOrder(() => _clientHandler.OnDeleteOrder(orderID, _clientID), instrumentID, state);
                        break;

                    case TriggerType.Immediate:
                    default:

                        _clientHandler.OnDeleteOrder(orderID, _clientID);
                        break;
                }
            }
        }

        public void New(NewOrder newOrder, OrderOption? option = null)
        {
            switch (option?.Trigger.When)
            {
                case TriggerType.Moment:

                    ScheduleOrderByMoment(() => _clientHandler.OnNewOrder(newOrder, _clientID), option?.Trigger.Moment ?? DateTime.Now);
                    break;

                case TriggerType.StateChange:

                    var state = option?.Trigger.StateChange ?? InstrumentState.Continous;
                    _clientHandler.FeedHandler.ScheduleOrder(() => _clientHandler.OnNewOrder(newOrder, _clientID), newOrder.InstrumentID, state);
                    break;

                case TriggerType.Immediate:
                default:

                    _clientHandler.OnNewOrder(newOrder, _clientID);
                    break;
            }
        }

        private void ScheduleOrderByMoment(Action action, DateTime moment)
        {
            var timer = new System.Timers.Timer(moment.Subtract(DateTime.Now).TotalMilliseconds);

            timer.Elapsed += (object? source, ElapsedEventArgs args) => action();
            timer.AutoReset = false;
            timer.Enabled = true;
        }

        private bool TryFindInstrumentIDByOrderID(string orderID, out string instrumentID)
        {
            foreach (ClientOrder order in _clientHandler[_clientID].Orders)
            {
                if (order.OriginalOrder.OrderID == orderID)
                {
                    instrumentID = order.OriginalOrder.InstrumentID;
                    return true;
                }
            }

            instrumentID = string.Empty;
            return false;
        }
    }
}
