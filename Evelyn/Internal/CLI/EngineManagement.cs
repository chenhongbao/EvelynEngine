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
using Evelyn.Internal;
using Evelyn.Model;
using Evelyn.Model.CLI;
using Evelyn.Plugin;
using Microsoft.Extensions.Logging;

namespace Evelyn.CLI
{
    internal class EngineManagement : IManagement
    {
        private readonly EvelynEngine _engine;

        internal EngineManagement(EvelynEngine engine)
        {
            _engine = engine;
        }

        public ManagementResult<AlterClientResult> AlterClient(string clientID, params string[] instrumentID)
        {
            try
            {
                _engine.AlterClient(clientID, instrumentID);
                return new ManagementResult<AlterClientResult>
                {
                    Result = new AlterClientResult { ClientID = clientID },
                    Description = new Description()
                };
            }
            catch (Exception ex)
            {
                return new ManagementResult<AlterClientResult>
                {
                    Result = new AlterClientResult { ClientID = clientID },
                    Description = new Description
                    {
                        Code = 21,
                        Message = ex.Message
                    }
                };
            }
        }

        public ManagementResult<ClientLogInformation> QueryClientLog(string clientID, DateTime afterTime, LogLevel logLevel = LogLevel.None)
        {
            var description = new Description();
            var information = new ClientLogInformation { ClientID = clientID, Level = logLevel };

            if (_engine.Handler.Clients.TryGetValue(clientID, out var client))
            {
                information.Logs = ((ClientLogger)client.Logger).Logs
                    .Where(log => log.LogLevel == logLevel && log.Timestamp.CompareTo(afterTime) > 0)
                    .ToList();
                information.Logs.Sort((lhs, rhs) => lhs.Timestamp.CompareTo(rhs.Timestamp));
            }
            else
            {
                description.Code = 22;
                description.Message = "No such client with ID " + clientID + ".";
            }

            return new ManagementResult<ClientLogInformation>
            {
                Result = information,
                Description = description
            };
        }

        public ManagementResult<ClientOrderInformation> QueryClientOrder(string clientID, string orderID)
        {
            if (_engine.Handler.Clients.TryGetValue(clientID, out var client))
            {
                if (client.Orders.TryGetValue(orderID, out var order))
                {
                    return new ManagementResult<ClientOrderInformation>
                    {
                        Result = new ClientOrderInformation
                        {
                            ClientID = clientID,
                            Order = order.OriginalOrder,
                            Trades = order.Trades.ToList(),
                            Status = order.Status
                        },
                        Description = new Description()
                    };
                }
                else
                {
                    return new ManagementResult<ClientOrderInformation>
                    {
                        Result = new ClientOrderInformation
                        {
                            ClientID = clientID
                        },
                        Description = new Description
                        {
                            Code = 23,
                            Message = "No such order with ID " + orderID + "."
                        }
                    };
                }
            }
            else
            {
                return new ManagementResult<ClientOrderInformation>
                {
                    Result = new ClientOrderInformation
                    {
                        ClientID = clientID
                    },
                    Description = new Description
                    {
                        Code = 24,
                        Message = "No such client with ID " + clientID + "."
                    }
                };
            }
        }

        public ManagementResult<ClientsInformation> QueryClients()
        {
            return new ManagementResult<ClientsInformation>
            {
                Result = new ClientsInformation
                {
                    Clients = _engine.Handler.Clients.Values.Select(client => new ClientBrief
                    {
                        ClientID = client.ClientID,
                        Subscription = client.Subscription.Instruments,
                        Orders = client.Orders.Values.Select(clientOrder => new ClientOrderBrief
                        {
                            ClientID = client.ClientID,
                            Order = clientOrder.OriginalOrder,
                            TradeQuantity = clientOrder.Trades.Select(trade => trade.TradeQuantity).Sum(),
                            AverageTradePrice = clientOrder.Trades.Select(trade => trade.TradePrice * trade.TradeQuantity).Sum() / clientOrder.Trades.Select(trade => trade.TradeQuantity).Sum(),
                            LastTradeTime = clientOrder.Trades.Last().TimeStamp,
                            Status = clientOrder.Status
                            
                        }).ToList()
                    }).ToList()
                },
                Description = new Description()
            };
        }

        public ManagementResult<EngineInformation> QueryEngineInformation()
        {
            try
            {
                return new ManagementResult<EngineInformation>
                {
                    Result = new EngineInformation
                    {
                        Broker = new BrokerInformation
                        {
                            IsConnected = _engine.Broker.IsConnected,
                            TradingDay = _engine.Broker.TradingDay
                        },
                        FeedSource = new FeedSourceInformation
                        {
                            IsConnected = _engine.FeedSource.IsConnected,
                            TradingDay = _engine.FeedSource.TradingDay,
                            OHLCGeneratorCount = _engine.FeedSource.Handler.OHLCGenerators.Count,
                            SubscriptionResponses = new Dictionary<string, bool>(_engine.FeedSource.Handler.SubscriptionResponses),
                            Instruments = new List<Instrument>(_engine.FeedSource.Handler.Instruments.Values),
                            ScheduledJobs = _engine.FeedSource.Handler.ScheduledJobs.Values.Select(job => new ScheduledJobBrief
                            {
                                JobID = job.JobID,
                                Name = job.Name,
                                InstrumentID = job.InstrumentID,
                                Option = job.Option,
                                SchedulingTime = job.SchedulingTime
                            }).ToList()
                        }
                    },
                    Description = new Description()
                };
            }
            catch (Exception ex)
            {
                return new ManagementResult<EngineInformation>
                {
                    Result = new EngineInformation(),
                    Description = new Description
                    {
                        Code = 25,
                        Message = ex.Message
                    }
                };
            }
        }
    }
}