﻿/*
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

namespace Evelyn.Extension.Simulator
{
    public class SimulatedBroker : IBroker
    {
        private readonly List<Bucket> _sellSide = new List<Bucket>();
        private readonly List<Bucket> _buySide = new List<Bucket>();
        private readonly ISet<string> _orderID = new HashSet<string>();

        private IOrderHandler? _handler;
        private IExchangeListener? _exchange;
        private DateOnly _tradingDay = DateOnly.MaxValue;
        private int _orderCounter = 0;
        private bool _connected = false;

        private IOrderHandler Handler => _handler ?? throw new NullReferenceException("Handler has no value.");
        private IExchangeListener Exchange => _exchange ?? throw new NullReferenceException("Exchange has no value.");

        public string NewOrderID => Interlocked.Increment(ref _orderCounter).ToString();
        public DateOnly TradingDay => _tradingDay;

        public void Delete(DeleteOrder delete)
        {
            if (TryDebook(delete, out var removed))
            {
                removed.Status = OrderStatus.Deleted;
                removed.Message = "Deleted";

                Handler.OnTrade(removed, new Description { Code = 0, Message = "OK" });
            }
        }

        private bool TryDebook(DeleteOrder delete, out Trade removed)
        {
            var sell = FindDeleted(delete, _sellSide);
            if (sell.Count() == 1)
            {
                /*
                 * Found the right order.
                 */
                if (!RemoveTrade(sell.First(), delete, out removed))
                {
                    Handler.OnTrade(
                        new Trade
                        {
                            InstrumentID = delete.InstrumentID,
                            OrderID = delete.OrderID,
                            TradingDay = TradingDay,
                            TimeStamp = DateTime.Now,
                            Price = double.MaxValue,
                            Quantity = int.MaxValue,
                            Direction = default(Direction),
                            Offset = default(Offset),
                            TradeID = String.Empty,
                            TradePrice = double.MaxValue,
                            TradeQuantity = int.MaxValue,
                            LeaveQuantity = int.MaxValue,
                            Status = OrderStatus.Deleted,
                            Message = "Removal failed"
                        },
                        new Description
                        {
                            Code = 1002,
                            Message = "Order found but can't be removed, " + delete.OrderID + "."
                        });
                    return false;
                }
            }
            else if (sell.Count() > 1)
            {
                Handler.OnTrade(
                    new Trade
                    {
                        InstrumentID = delete.InstrumentID,
                        OrderID = delete.OrderID,
                        TradingDay = TradingDay,
                        TimeStamp = DateTime.Now,
                        Price = double.MaxValue,
                        Quantity = int.MaxValue,
                        Direction = default(Direction),
                        Offset = default(Offset),
                        TradeID = String.Empty,
                        TradePrice = double.MaxValue,
                        TradeQuantity = int.MaxValue,
                        LeaveQuantity = int.MaxValue,
                        Status = OrderStatus.Deleted,
                        Message = "Found more than one order to delete"
                    },
                    new Description
                    {
                        Code = 1003,
                        Message = "Found more than one order to delete with ID " + delete.OrderID + "."
                    });

                removed = new Trade();
                return false;
            }
            else
            {
                var buy = FindDeleted(delete, _buySide);
                if (buy.Count() == 1)
                {
                    if (!RemoveTrade(buy.First(), delete, out removed))
                    {
                        Handler.OnTrade(
                            new Trade
                            {
                                InstrumentID = delete.InstrumentID,
                                OrderID = delete.OrderID,
                                TradingDay = TradingDay,
                                TimeStamp = DateTime.Now,
                                Price = double.MaxValue,
                                Quantity = int.MaxValue,
                                Direction = default(Direction),
                                Offset = default(Offset),
                                TradeID = String.Empty,
                                TradePrice = double.MaxValue,
                                TradeQuantity = int.MaxValue,
                                LeaveQuantity = int.MaxValue,
                                Status = OrderStatus.Deleted,
                                Message = "Removal failed"
                            },
                            new Description
                            {
                                Code = 1002,
                                Message = "Order found but can't be removed, " + delete.OrderID + "."
                            });
                        return false;
                    }
                }
                else if (buy.Count() > 1)
                {
                    Handler.OnTrade(
                        new Trade
                        {
                            InstrumentID = delete.InstrumentID,
                            OrderID = delete.OrderID,
                            TradingDay = TradingDay,
                            TimeStamp = DateTime.Now,
                            Price = double.MaxValue,
                            Quantity = int.MaxValue,
                            Direction = default(Direction),
                            Offset = default(Offset),
                            TradeID = String.Empty,
                            TradePrice = double.MaxValue,
                            TradeQuantity = int.MaxValue,
                            LeaveQuantity = int.MaxValue,
                            Status = OrderStatus.Deleted,
                            Message = "Found more than one order to delete"
                        },
                        new Description
                        {
                            Code = 1003,
                            Message = "Found more than one order to delete with ID " + delete.OrderID + "."
                        });

                    removed = new Trade();
                    return false;
                }
                else
                {
                    Handler.OnTrade(
                        new Trade
                        {
                            InstrumentID = delete.InstrumentID,
                            OrderID = delete.OrderID,
                            TradingDay = TradingDay,
                            TimeStamp = DateTime.Now,
                            Price = double.MaxValue,
                            Quantity = int.MaxValue,
                            Direction = default(Direction),
                            Offset = default(Offset),
                            TradeID = String.Empty,
                            TradePrice = double.MaxValue,
                            TradeQuantity = int.MaxValue,
                            LeaveQuantity = int.MaxValue,
                            Status = OrderStatus.Deleted,
                            Message = "No such order"
                        },
                        new Description
                        {
                            Code = 1004,
                            Message = "No such order with ID " + delete.OrderID + "."
                        });

                    removed = new Trade();
                    return false;
                }
            }

            return true;
        }

        private bool RemoveTrade(Bucket bucket, DeleteOrder delete, out Trade removed)
        {
            foreach(var trade in bucket.Orders)
            {
                if (trade.OrderID == delete.OrderID && trade.InstrumentID == delete.InstrumentID)
                {
                    removed = trade;
                    return true;
                }
            }

            removed = new Trade();
            return false;
        }

        private IEnumerable<Bucket> FindDeleted(DeleteOrder delete, List<Bucket> buckets)
        {
            return buckets.Where(bucket => bucket.Orders.Where(trade => trade.InstrumentID == delete.InstrumentID && trade.OrderID == delete.OrderID).Count() > 0);
        }

        public void New(NewOrder order)
        {
            if (_orderID.Contains(order.OrderID))
            {
                Handler.OnTrade(
                    new Trade
                    {
                        InstrumentID = order.InstrumentID,
                        OrderID = order.OrderID,
                        TradingDay = TradingDay,
                        TimeStamp = DateTime.Now,
                        Price = order.Price,
                        Quantity = order.Quantity,
                        Direction = order.Direction,
                        Offset = order.Offset,
                        TradeID = String.Empty,
                        TradePrice = double.MaxValue,
                        TradeQuantity = int.MaxValue,
                        LeaveQuantity = int.MaxValue,
                        Status = OrderStatus.Rejected,
                        Message = "Duplicated order"
                    },
                    new Description
                    {
                        Code = 1004,
                        Message = "Duplicated order with ID " + order.OrderID + "."
                    });
            }
            else
            {
                Enbook(order);
            }
        }

        private void Enbook(NewOrder newOrder)
        {
            if (newOrder.Direction == Direction.Buy)
            {
                Enbook(newOrder, _buySide);
            }
            else
            {
                Enbook(newOrder, _sellSide);
            }
        }

        private void Enbook(NewOrder newOrder, List<Bucket> buckets)
        {
            var foundBuckets = buckets.Where(bucket => bucket.Price == newOrder.Price);
            if (foundBuckets.Count() == 0)
            {
                buckets.Add(new Bucket
                {
                    Price = newOrder.Price,
                    Orders = new List<Trade>
                    {
                        new Trade
                        {
                            // TODO Move data from order to trade.
                        }
                    }
                });
            }
            else if (foundBuckets.Count() == 1)
            {
                foundBuckets.First().Orders.Add(new Trade
                {
                    // TODO Move data from order to trade.
                });
            }
            else
            {
                throw new InvalidDataException("Duplicated bucket for price " + newOrder.Price + ".");
            }
        }

        public void Register(IOrderHandler orderHandler, IExchangeListener exchangeListener)
        {
            _handler = orderHandler;
            _exchange = exchangeListener;
        }

        internal void Match(Tick tick)
        {
            _tradingDay = tick.TradingDay;
            if (!_connected)
            {
                _connected = true;
                Exchange.OnConnected(_connected);
            }

            MatchBook(tick);
        }

        private void MatchBook(Tick tick)
        {
            // TODO Match book.
        }
    }
}