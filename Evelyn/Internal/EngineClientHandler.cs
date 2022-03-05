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
using System.Collections.Concurrent;

namespace Evelyn.Internal
{
    internal class EngineClientHandler : IClientHandler
    {
        private readonly ConcurrentDictionary<string, Client> _clients = new ConcurrentDictionary<string, Client>();

        private EngineBroker? _broker;
        private EngineFeedSource? _feedSource;
        private EngineFeedHandler? _feedHandler;

        private ILogger Logger { get; init; } = Loggers.CreateLogger(nameof(EngineClientHandler));

        internal EngineBroker Broker => _broker ?? throw new NullReferenceException("Engine broker has no value.");

        internal EngineFeedSource FeedSource => _feedSource ?? throw new NullReferenceException("Enginefeed source has no value.");

        internal EngineFeedHandler FeedHandler => _feedHandler ?? throw new NullReferenceException("Engine feed handler has no value.");

        internal List<Client> Clients => new List<Client>(_clients.Values);

        internal Client this[string clientID]
        {
            get => _clients[clientID];
        }

        internal bool HasClient(string clientID)
        {
            return _clients.ContainsKey(clientID);
        }

        public void OnClientConnect(string clientID, IClientService service)
        {
            if (!_clients.TryAdd(clientID, new Client(service, clientID)))
            {
                Logger.LogWarning("{0}\n{1}", "Another client exists with ID " + clientID + ".", new System.Diagnostics.StackTrace().ToString());
            }
        }

        internal void OnClientConnect(string clientID, IAlgorithm algorithm, IClientService service)
        {
            if (!_clients.TryAdd(clientID, new Client(service, clientID, algorithm)))
            {
                Logger.LogWarning("{0}\n{1}", "Another client exists with ID " + clientID + ".", new System.Diagnostics.StackTrace().ToString());
            }
        }

        public void OnClientDisconnect(string clientID)
        {
            if (!_clients.ContainsKey(clientID))
            {
                Logger.LogInformation("{0}\n{1}", "No such client with ID " + clientID + ".", new System.Diagnostics.StackTrace().ToString());
                return;
            }

            try
            {
                FeedSource.Subscribe(_clients[clientID].Subscription.Instruments, false);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("{0}\n{1}", "Unsubscribe instrument while client is disconnected and throws exception, " + ex.Message, ex.StackTrace?.ToString());
            }
            finally
            {
                _clients.Remove(clientID, out var _);
            }
        }

        public void OnDeleteOrder(DeleteOrder deleteOrder, string clientID)
        {
            if (!_clients.TryGetValue(clientID, out var client))
            {
                Logger.LogInformation("{0}\n{1}", "No such client with ID " + clientID + ".", new System.Diagnostics.StackTrace().ToString());
                return;
            }

            if (!Broker.IsConnected)
            {
                /*
                 * If exchange is disconnected, can't delete an order so return error.
                 */
                try
                {
                    client.Service.SendTrade(
                        new Trade
                        {
                            InstrumentID = deleteOrder.InstrumentID,
                            TradingDay = DateOnly.MaxValue,
                            TimeStamp = DateTime.MaxValue,
                            OrderID = deleteOrder.OrderID,
                            Price = double.MaxValue,
                            Quantity = int.MaxValue,
                            Direction = default(Direction),
                            Offset = default(Offset),
                            TradeID = String.Empty,
                            TradePrice = double.MaxValue,
                            TradeQuantity = int.MaxValue,
                            LeaveQuantity = int.MaxValue,
                            Status = OrderStatus.Deleted,
                            Message = "Exchange disconnected."
                        },
                        new Description
                        {
                            Code = 1,
                            Message = "Exchange is disconnected."
                        },
                        clientID);
                }
                catch (Exception ex)
                {
                    Logger.LogError("{0}\n{1}", "Send trade to client when exchange is disconnected and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                }

                return;
            }

            if (client.Orders.TryGetValue(deleteOrder.OrderID, out var order))
            {
                try
                {
                    Broker.Delete(order.RewriteDeleteOrder);
                }
                catch (Exception ex)
                {
                    Logger.LogError("{0}\n{1}", "Broker deletes order and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                }
            }
            else
            {
                /*
                 * Sends error response when no order found for the given order ID.
                 */
                try
                {
                    _clients[clientID].Service.SendTrade(
                        new Trade
                        {
                            InstrumentID = deleteOrder.InstrumentID,
                            TradingDay = DateOnly.MaxValue,
                            TimeStamp = DateTime.MaxValue,
                            OrderID = deleteOrder.OrderID,
                            Price = double.MaxValue,
                            Quantity = int.MaxValue,
                            Direction = default(Direction),
                            Offset = default(Offset),
                            TradeID = String.Empty,
                            TradePrice = double.MaxValue,
                            TradeQuantity = int.MaxValue,
                            LeaveQuantity = int.MaxValue,
                            Status = OrderStatus.Deleted,
                            Message = "No such order."
                        },
                        new Description
                        {
                            Code = 2,
                            Message = "No such order with ID " + deleteOrder.OrderID + "."
                        },
                        clientID);
                }
                catch (Exception ex)
                {
                    Logger.LogError("{0}\n{1}", "Send trade to client when no such order to delete and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                }
            }
        }

        public void OnNewOrder(NewOrder newOrder, string clientID)
        {
            /*
             * Rewrite the order ID, and save order information into ClientOrder.
             */
            if (!_clients.TryGetValue(clientID, out var client))
            {
                Logger.LogInformation("{0}\n{1}", "No such client with ID " + clientID + ".", new System.Diagnostics.StackTrace().ToString());
                return;
            }

            if (!Broker.IsConnected)
            {
                /*
                 * When exchange is disconnected, can't request order so return error.
                 */
                try
                {
                    client.Service.SendTrade(
                        new Trade
                        {
                            InstrumentID = newOrder.InstrumentID,
                            TradingDay = newOrder.TradingDay ?? Broker.TradingDay,
                            TimeStamp = newOrder.TimeStamp ?? DateTime.Now,
                            OrderID = newOrder.OrderID,
                            Price = newOrder.Price,
                            Quantity = newOrder.Quantity,
                            Direction = newOrder.Direction,
                            Offset = newOrder.Offset,
                            TradeID = String.Empty,
                            TradePrice = double.MaxValue,
                            TradeQuantity = int.MaxValue,
                            LeaveQuantity = newOrder.Quantity,
                            Status = OrderStatus.Rejected,
                            Message = "Exchange disconnected."
                        },
                        new Description
                        {
                            Code = 11,
                            Message = "Exchange is disconnected."
                        },
                        clientID);
                }
                catch (Exception ex)
                {
                    Logger.LogError("{0}\n{1}", "Send trade to client when exchange is disconnected and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                }
            }
            else
            {
                var clientOrder = new ClientOrder(newOrder, Broker.NewOrderID);

                if (!client.Orders.TryAdd(newOrder.OrderID, clientOrder))
                {
                    /*
                     * Duplicated client's order ID, don't send the order because responses confuse.
                     */
                    try
                    {
                        client.Service.SendTrade(
                            new Trade
                            {
                                InstrumentID = newOrder.InstrumentID,
                                TradingDay = newOrder.TradingDay ?? Broker.TradingDay,
                                TimeStamp = newOrder.TimeStamp ?? DateTime.Now,
                                OrderID = newOrder.OrderID,
                                Price = newOrder.Price,
                                Quantity = newOrder.Quantity,
                                Direction = newOrder.Direction,
                                Offset = newOrder.Offset,
                                TradeID = String.Empty,
                                TradePrice = double.MaxValue,
                                TradeQuantity = int.MaxValue,
                                LeaveQuantity = newOrder.Quantity,
                                Status = OrderStatus.Rejected,
                                Message = "Duplicated order ID."
                            },
                            new Description
                            {
                                Code = 12,
                                Message = "Duplicated order ID."
                            },
                            clientID);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("{0}\n{1}", "Send trade to client when order is duplicated and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                    }
                }
                else
                {
                    try
                    {
                        Broker.NewOrder(clientOrder.RewriteNewOrder);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("{0}\n{1}", "Broker requests new order and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                    }
                }
            }
        }

        public void OnSubscribe(string instrumentID, bool isSubscribed, string clientID)
        {
            if (!_clients.ContainsKey(clientID))
            {
                Logger.LogInformation("{0}\n{1}", "No such client with ID " + clientID + ".", new System.Diagnostics.StackTrace().ToString());
                return;
            }

            var client = _clients[clientID];
            var currentInstruments = client.Subscription.Instruments;

            /*
             * Response to subscription of the instrument shall be routed to this client.
             */
            client.Subscription.MarkSubscriptionResponse(instrumentID, waitResponse: true);

            if (isSubscribed)
            {
                if (currentInstruments.Contains(instrumentID))
                {
                    /*
                     * If instrument has been subscribed, nothing happens, and sends a response with error.
                     */
                    try
                    {
                        FeedHandler.OnSubscribed(
                            instrumentID, new Description
                            {
                                Code = 101,
                                Message = "Duplicated subscription for " + instrumentID + "."
                            },
                            true);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("{0}\n{1}", "Call feed handler for subscription response and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                    }
                }
                else
                {
                    try
                    {
                        FeedSource.Subscribe(new List<string> { instrumentID }, true);
                        client.Subscription.Subscribe(instrumentID, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("{0}\n{1}", "Subscribe instrument and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                    }
                }
            }
            else
            {
                if (!currentInstruments.Contains(instrumentID))
                {
                    /*
                     * If instrument has been unsubscribed or never subscribed, nothing happens, and sends a response with error.
                     */
                    try
                    {
                        FeedHandler.OnSubscribed(instrumentID, new Description { Code = 102, Message = " No such subscription for " + instrumentID + "." }, false);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("{0}\n{1}", "Unsubscribe unknown instrument and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                    }
                }
                else
                {
                    try
                    {
                        FeedSource.Subscribe(new List<string> { instrumentID }, false);
                        client.Subscription.Subscribe(instrumentID, false);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("{0}\n{1}", "Unsubscribe instrument and throws exception, " + ex.Message, ex.StackTrace?.ToString());
                    }
                }
            }
        }

        internal void Configure(EngineBroker broker, EngineFeedSource feedSource, EngineFeedHandler feedHandler)
        {
            _broker = broker;
            _feedSource = feedSource;
            _feedHandler = feedHandler;
        }
    }
}