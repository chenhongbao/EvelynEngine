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
using Evelyn.Internal.Logging;
using Evelyn.Model;
using Evelyn.Model.CLI;
using Evelyn.Model.Logging;
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
                        Code = ErrorCodes.AlterClientThrowsException,
                        Message = ex.Message
                    }
                };
            }
        }

        public ManagementResult<bool> ExitSystem()
        {
            try
            {
                _engine.Handler.Clients.Values.ToList().ForEach(client =>
                {
                    _engine.AlterClient(client.ClientID);
                    _engine.DeregisterClient(client.ClientID);
                });
                return new ManagementResult<bool>
                {
                    Result = true,
                    Description = new Description
                    {
                        Code = ErrorCodes.OK
                    }
                };
            }
            catch (Exception e)
            {
                return new ManagementResult<bool>
                {
                    Result = false,
                    Description = new Description
                    {
                        Code = ErrorCodes.ExitSystemThrowsException,
                        Message = e.Message
                    }
                };
            }
        }

        public ManagementResult<ClientLogInformation> QueryClientLog(string clientID, DateTime afterTime, LogLevel logLevel = LogLevel.None)
        {
            var information = new ClientLogInformation { ClientID = clientID, Level = logLevel, LastLogTime = DateTime.MaxValue };

            if (_engine.Handler.Clients.TryGetValue(clientID, out var client))
            {
                information.Logs = ((ClientLogger)client.Logger).Logs
                    .Where(log => (log.LogLevel == logLevel || logLevel == LogLevel.None) && log.Timestamp.CompareTo(afterTime) > 0)
                    .ToList();
                information.Logs.Sort((lhs, rhs) => lhs.Timestamp.CompareTo(rhs.Timestamp));

                if (information.Logs.Count > 0)
                {
                    information.LastLogTime = information.Logs.Last().Timestamp;
                }

                return new ManagementResult<ClientLogInformation>
                {
                    Result = information,
                    Description = new Description
                    {
                        Code = ErrorCodes.OK
                    }
                };
            }
            else
            {
                information.Logs = new List<ClientLog>();

                return new ManagementResult<ClientLogInformation>
                {
                    Result = information,
                    Description = new Description
                    {
                        Code = ErrorCodes.NoSuchClient,
                        Message = "No such client with ID " + clientID + "."
                    }
                };
            }
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
                            Code = ErrorCodes.NoSuchOrder,
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
                        Code = ErrorCodes.NoSuchClient,
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
                        Orders = client.Orders.Values.Select(clientOrder =>
                        {
                            var brief = new ClientOrderBrief
                            {
                                ClientID = client.ClientID,
                                Order = clientOrder.OriginalOrder,
                                TradeQuantity = clientOrder.Trades.Select(trade => trade.TradeQuantity).Sum(),
                                Status = clientOrder.Status
                            };

                            if (brief.TradeQuantity > 0)
                            {
                                brief.AverageTradePrice = clientOrder.Trades.Select(trade => trade.TradePrice * trade.TradeQuantity).Sum() / clientOrder.Trades.Select(trade => trade.TradeQuantity).Sum();
                                brief.LastTradeTime = clientOrder.Trades.Last().TimeStamp;
                            }

                            return brief;
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
                                ClientID = job.ClientID,
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
                        Code = ErrorCodes.QueryEngineThrowsException,
                        Message = ex.Message
                    }
                };
            }
        }

        public ManagementResult<string> SendCommand(string clientID, params string[] commands)
        {
            if (_engine.Handler.Clients.TryGetValue(clientID, out var client))
            {
                try
                {
                    if (client.Algorithm == null)
                    {
                        return new ManagementResult<string>
                        {
                            Description = new Description
                            {
                                Code = ErrorCodes.NoLocalClient,
                                Message = "Client is not local client with ID " + clientID + "."
                            },
                            Result = String.Empty
                        };
                    }
                    else
                    {
                        return new ManagementResult<string>
                        {
                            Description = new Description
                            {
                                Code = ErrorCodes.OK
                            },
                            Result = client.Algorithm.OnCommand(commands)
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new ManagementResult<string>
                    {
                        Description = new Description
                        {
                            Code = ErrorCodes.CommandThrowsException,
                            Message = "Invoke command throws exception: " + ex.Message + "."
                        },
                        Result = String.Empty
                    };
                }
            }
            else
            {
                return new ManagementResult<string>
                {
                    Description = new Description
                    {
                        Code = ErrorCodes.NoSuchClient,
                        Message = "No such client " + clientID + "."
                    },
                    Result = String.Empty
                };
            }
        }
    }
}
