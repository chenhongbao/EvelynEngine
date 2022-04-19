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
using Evelyn.Plugin;

namespace Evelyn.Extension.Simulator
{
    public class SimulatedBroker : IBroker
    {
        private readonly List<Bucket> _sellSide = new List<Bucket>();
        private readonly List<Bucket> _buySide = new List<Bucket>();
        private readonly ISet<string> _orderID = new HashSet<string>();
        private readonly Queue<DeleteOrder> _deletion = new Queue<DeleteOrder>();

        private IOrderHandler? _handler;
        private IExchangeListener? _exchange;
        private DateOnly _tradingDay = DateOnly.MaxValue;
        private int _orderCounter = 0;

        internal IExchangeListener Exchange => _exchange ?? throw new NullReferenceException("Exchange has no value.");
        private IOrderHandler Handler => _handler ?? throw new NullReferenceException("Handler has no value.");

        public string NewOrderID => Interlocked.Increment(ref _orderCounter).ToString();
        public DateOnly TradingDay => _tradingDay;

        public void Delete(DeleteOrder delete)
        {
            _deletion.Enqueue(delete);
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
                    CallRemovalFailure(delete);
                    return false;
                }
            }
            else if (sell.Count() > 1)
            {
                CallDupOrder(delete);

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
                        CallRemovalFailure(delete);
                        return false;
                    }
                }
                else if (buy.Count() > 1)
                {
                    CallDupOrder(delete);

                    removed = new Trade();
                    return false;
                }
                else
                {
                    CallNoSuchOrder(delete);

                    removed = new Trade();
                    return false;
                }
            }

            return true;
        }

        private void CallNoSuchOrder(DeleteOrder delete)
        {
            var trade = InitDeleteTrade(delete);
            trade.Message = "No such order";

            TryCatch(() => Handler.OnTrade(
                trade,
                new Description
                {
                    Code = ErrorCodes.SimBrokerNoSuchOrder,
                    Message = "No such order with ID " + delete.OrderID + "."
                }));
        }

        private void CallDupOrder(DeleteOrder delete)
        {
            var trade = InitDeleteTrade(delete);
            trade.Message = "Internal error: found more than one order to delete";

            TryCatch(() => Handler.OnTrade(
                trade,
                new Description
                {
                    Code = ErrorCodes.SimBrokerDuplicatedOrders,
                    Message = "Found more than one order to delete with ID " + delete.OrderID + "."
                }));
        }

        private void CallRemovalFailure(DeleteOrder delete)
        {
            var trade = InitDeleteTrade(delete);
            trade.Message = "Internal error: removal failed";

            TryCatch(() => Handler.OnTrade(
                trade,
                new Description
                {
                    Code = ErrorCodes.SimBrokerDeletionFail,
                    Message = "Order found but can't be removed, " + delete.OrderID + "."
                }));
        }

        private Trade InitDeleteTrade(DeleteOrder delete)
        {
            return new Trade
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
                Status = OrderStatus.None,
                Message = String.Empty
            };
        }

        private bool RemoveTrade(Bucket bucket, DeleteOrder delete, out Trade removed)
        {
            ExecutingOrder? toRemove = null;
            Trade removeTrade = default;

            foreach (var exec in bucket.Orders)
            {
                if (exec.OriginalOrder.OrderID == delete.OrderID && exec.OriginalOrder.InstrumentID == delete.InstrumentID)
                {
                    removeTrade = InitializeTrade(exec.OriginalOrder);
                    removeTrade.LeaveQuantity = exec.LeaveQuantity;

                    toRemove = exec;
                    break;
                }
            }

            if (toRemove != null)
            {
                bucket.Orders.Remove(toRemove);
                removed = removeTrade;
                return true;
            }
            else
            {
                removed = new Trade();
                return false;
            }
        }

        internal void Delete()
        {
            while (_deletion.Count > 0)
            {
                var delete = _deletion.Dequeue();
                if (TryDebook(delete, out var removed))
                {
                    removed.TradeQuantity = 0;
                    removed.Status = OrderStatus.Deleted;
                    removed.Message = "Deleted";

                    TryCatch(() => Handler.OnTrade(removed, new Description { Code = ErrorCodes.OK, Message = "OK" }));
                }
            }
        }

        private IEnumerable<Bucket> FindDeleted(DeleteOrder delete, List<Bucket> buckets)
        {
            return buckets.Where(bucket => bucket.Orders.Where(exec => exec.OriginalOrder.InstrumentID == delete.InstrumentID && exec.OriginalOrder.OrderID == delete.OrderID).Count() > 0);
        }

        public void New(NewOrder order)
        {
            if (_orderID.Contains(order.OrderID))
            {
                TryCatch(() => Handler.OnTrade(
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
                        Code = ErrorCodes.SimBrokerDuplicatedOrders,
                        Message = "Duplicated order with ID " + order.OrderID + "."
                    }));
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

        private void Enbook(NewOrder order, List<Bucket> buckets)
        {
            var foundBuckets = buckets.Where(bucket => bucket.Price == order.Price);
            if (foundBuckets.Count() == 0)
            {
                buckets.Add(new Bucket
                {
                    Price = order.Price,
                    Orders = new List<ExecutingOrder> { new ExecutingOrder { OriginalOrder = order, LeaveQuantity = order.Quantity } }
                });
            }
            else if (foundBuckets.Count() == 1)
            {
                foundBuckets.First().Orders.Add(new ExecutingOrder { OriginalOrder = order, LeaveQuantity = order.Quantity });
            }
            else
            {
                throw new InvalidDataException("Duplicated bucket for price " + order.Price + ".");
            }
        }

        private Trade InitializeTrade(NewOrder order)
        {
            return new Trade
            {
                InstrumentID = order.InstrumentID,
                ExchangeID = order.ExchangeID,
                Symbol = order.Symbol,
                OrderID = order.OrderID,
                TradingDay = TradingDay,
                TimeStamp = DateTime.MaxValue,
                Price = order.Price,
                Quantity = order.Quantity,
                Direction = order.Direction,
                Offset = order.Offset,
                TradeID = String.Empty,
                TradePrice = double.MaxValue,
                TradeQuantity = 0,
                LeaveQuantity = order.Quantity,
                Status = OrderStatus.None,
                Message = "None"
            };
        }

        public void Register(IOrderHandler orderHandler, IExchangeListener exchangeListener)
        {
            _handler = orderHandler;
            _exchange = exchangeListener;
        }

        internal void Match(Tick tick)
        {
            _tradingDay = tick.TradingDay;
            MatchBook(tick);
        }

        private void MatchBook(Tick tick)
        {
            /*
             * Sort buckets by price from high to low.
             */
            _sellSide.Where(bucket => bucket.Price <= tick.BidPrice).ToList()
                .ForEach(bucket =>
                {
                    bucket.Orders.ForEach(exec =>
                    {
                        /*
                         * Copy by value.
                         */
                        Trade trade = InitializeTrade(exec.OriginalOrder);
                        trade.TradePrice = tick.BidPrice ?? throw new InvalidDataException("Bid price has no value.");

                        /*
                         * Trade order according to bid volume. 
                         */
                        if (exec.LeaveQuantity <= tick.BidVolume)
                        {
                            /*
                             * Completed.
                             */
                            trade.TradeQuantity = exec.LeaveQuantity;
                            trade.LeaveQuantity -= trade.TradeQuantity;
                            trade.Status = OrderStatus.Completed;
                            trade.Message = "Completed";

                            /*
                             * Update order.
                             */
                            exec.LeaveQuantity = 0;
                        }
                        else
                        {
                            trade.TradeQuantity = (int)tick.BidVolume;
                            trade.LeaveQuantity -= trade.TradeQuantity;
                            trade.Status = OrderStatus.Trading;
                            trade.Message = "Trading";

                            exec.LeaveQuantity = trade.LeaveQuantity;
                        }

                        trade.TradeID = trade.OrderID + (trade.Quantity - trade.LeaveQuantity).ToString("{0:D2}");
                        trade.TimeStamp = tick.TimeStamp;

                        TryCatch(() => Handler.OnTrade(trade, new Description { }));
                    });

                    bucket.Orders.RemoveAll(order => order.LeaveQuantity == 0);
                });

            /*
             * Sort buckets by price from low to high.
             */
            _buySide.Where(bucket => bucket.Price >= tick.AskPrice).ToList()
                .ForEach(bucket =>
                {
                    bucket.Orders.ForEach(exec =>
                    {
                        /*
                         * Copy by value.
                         */
                        Trade trade = InitializeTrade(exec.OriginalOrder);
                        trade.TradePrice = tick.AskPrice ?? throw new InvalidDataException("Ask price has no value.");

                        /*
                         * Trade order according to bid volume. 
                         */
                        if (exec.LeaveQuantity <= tick.AskVolume)
                        {
                            /*
                             * Completed.
                             */
                            trade.TradeQuantity = exec.LeaveQuantity;
                            trade.LeaveQuantity = 0;
                            trade.Status = OrderStatus.Completed;
                            trade.Message = "Completed";

                            /*
                             * Update order.
                             */
                            exec.LeaveQuantity = 0;
                        }
                        else
                        {
                            exec.LeaveQuantity -= (int)tick.AskVolume;

                            trade.TradeQuantity = (int)tick.AskVolume;
                            trade.LeaveQuantity = exec.LeaveQuantity;
                            trade.Status = OrderStatus.Trading;
                            trade.Message = "Trading";
                        }

                        /*
                         * Last total trade quantity as suffix of trade ID.
                         */
                        trade.TradeID = trade.OrderID + (trade.Quantity - trade.LeaveQuantity).ToString("{0:D2}");
                        trade.TimeStamp = tick.TimeStamp;

                        TryCatch(() => Handler.OnTrade(trade, new Description { }));
                    });

                    bucket.Orders.RemoveAll(order => order.LeaveQuantity == 0);
                });
        }

        private void TryCatch(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
}