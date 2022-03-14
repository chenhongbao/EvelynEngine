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

namespace Evelyn.Extension.Simulator
{
    public class SimulatedBroker : IBroker
    {
        private readonly List<Bucket> _sellSide = new List<Bucket>();
        private readonly List<Bucket> _buySide = new List<Bucket>();

        private IOrderHandler? _handler;
        private IExchangeListener? _exchange;
        private DateOnly _tradingDay = DateOnly.MaxValue;
        private int _orderCounter = 0;
        private bool _connected = false;

        private IOrderHandler Handler => _handler ?? throw new NullReferenceException("Handler has no value.");
        private IExchangeListener Exchange => _exchange ?? throw new NullReferenceException("Exchange has no value.");

        public string NewOrderID => Interlocked.Increment(ref _orderCounter).ToString();
        public DateOnly TradingDay => _tradingDay;

        public void Delete(DeleteOrder deleteOrder)
        {
            if (!TryDebook(deleteOrder))
            {
                Handler.OnTrade(
                    new Trade
                    {
                        InstrumentID = deleteOrder.InstrumentID,
                        OrderID = deleteOrder.OrderID,
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
                        Code = 1002,
                        Message = "No such order with ID " + deleteOrder.OrderID + "."
                    });
            }
        }

        private bool TryDebook(DeleteOrder delete)
        {
            var sell = FindDeleted(delete, _sellSide);
            if (sell.Count() == 1)
            {
                // TODO Found the right order.
            }
            else if (sell.Count() > 1)
            {
                // TODO Duplicated prders.
            }
            else
            {
                var buy = FindDeleted(delete, _buySide);
                if (buy.Count() == 1)
                {
                    // TODO Found the right order.
                }
                else if (buy.Count() > 1)
                {
                    // TODO Duplicated prders.
                }
            }

            // TODO No order.
            return false;
        }

        private IEnumerable<Trade> FindDeleted(DeleteOrder delete, List<Bucket> buckets)
        {
            return buckets.Select(bucket => bucket.Orders.Where(trade => trade.InstrumentID == delete.InstrumentID && trade.OrderID == delete.OrderID))
                .Where(trades => trades.Count() > 0)
                .Select(trades => trades.First());
        }

        public void New(NewOrder newOrder)
        {
            Enbook(newOrder);
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
                var bucket = foundBuckets.First();
                bucket.Orders.Add(new Trade
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