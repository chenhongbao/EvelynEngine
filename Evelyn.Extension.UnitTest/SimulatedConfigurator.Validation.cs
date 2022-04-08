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
using Evelyn.Extension.Simulator;
using Evelyn.Model;
using Evelyn.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Evelyn.Extension.UnitTest
{
    [TestClass]
    public class SimulatedConfiguratorValidation : SimulatedConfiguratorData
    {
        private SimulatedBroker Broker { get; set; } = null;
        private SimulatedFeedSource FeedSource { get; set; } = null;

        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();
        }

        [TestMethod("Subcribe and receive ticks.")]
        public void ReceiveTicks()
        {
            new SimulatedConfigurator(Ticks, Instruments).Configure(out var broker, out var feedSource);

            Broker = (SimulatedBroker)broker;
            Broker.Register(OrderHandler, BrokerExchange);

            FeedSource = (SimulatedFeedSource)feedSource;
            FeedSource.Register(FeedHandler, FeedSourceExchange);

            /*
             * Subscribe an instrument from feed source and handler receives only that instrument.
             * 
             * 1. Handler receives feeds of the subscribed instrument.
             * 2. Exchange listener receives connect status notice.
             * 3. Unsubscribe instrument and receive respones.
             */
            FeedSource.Register(FeedHandler, FeedSourceExchange);
            FeedSource.Subscribe("l2205");

            /*
             * Check receive subscription responses.
             */
            Assert.AreEqual(1, FeedHandler.Subscriptions.Count);
            Assert.AreEqual("l2205", FeedHandler.Subscriptions[0].Item1);
            Assert.IsTrue(FeedHandler.Subscriptions[0].Item3);

            /*
             * Then subscribe an non existing instrument and receive error response.
             */
            FeedSource.Subscribe("ANY_ID");

            Assert.AreEqual(2, FeedHandler.Subscriptions.Count);
            Assert.AreEqual("ANY_ID", FeedHandler.Subscriptions[1].Item1);
            Assert.AreNotEqual(0, FeedHandler.Subscriptions[1].Item2.Code);
            Assert.IsFalse(FeedHandler.Subscriptions[1].Item3);

            /*
             * 1. Mock feeds, send the first instrument status update.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.IsTrue(FeedSourceExchange.Connected);

            /*
             * Feed handler receives first instrument feed.
             */
            Assert.AreEqual(1, FeedHandler.Instruments.Count);
            Assert.AreEqual("l2205", FeedHandler.Instruments[0].InstrumentID);
            Assert.AreEqual(InstrumentStatus.Continous, FeedHandler.Instruments[0].Status);

            /*
             * Mock the second instrument status update.
             */
            Assert.IsTrue(FeedSource.Flop());

            /*
             * Feed handler doesn't receive the second instrument update because
             * it doesn't subscribe it.
             */
            Assert.AreEqual(1, FeedHandler.Instruments.Count);

            /*
             * 2. Mock feeds, ticks are received.
             */
            for (int i = 0; i < Ticks.Count; ++i)
            {
                Assert.IsTrue(FeedSource.Flop());
            }

            /*
             * Check handler receives correct ticks.
             */
            var correctTicks = Ticks.Where(tick => tick.InstrumentID == "l2205").ToList();
            Assert.AreEqual(correctTicks.Count(), FeedHandler.Ticks.Count);

            for (int i = 0; i < FeedHandler.Ticks.Count; ++i)
            {
                Assert.AreEqual(correctTicks[i], FeedHandler.Ticks[i]);
            }

            /*
             * 3. Mock last two instrument status updates.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.IsTrue(FeedSource.Flop());

            /*
             * Check handler receives the last two instrument statuses.
             */
            Assert.AreEqual(2, FeedHandler.Instruments.Count);
            Assert.AreEqual("l2205", FeedHandler.Instruments[1].InstrumentID);
            Assert.AreEqual(InstrumentStatus.Closed, FeedHandler.Instruments[1].Status);

            /*
             * All feeds are sent, exchange is disconnected.
             */
            Assert.IsFalse(FeedSource.Flop());
            Assert.IsFalse(FeedSourceExchange.Connected);

            /*
             * 4. Unsubscribe instrument.
             */
            FeedSource.Unsubscribe("l2205");

            Assert.AreEqual(3, FeedHandler.Subscriptions.Count);
            Assert.AreEqual("l2205", FeedHandler.Subscriptions[2].Item1);
            Assert.AreEqual(0, FeedHandler.Subscriptions[2].Item2.Code);
            Assert.IsFalse(FeedHandler.Subscriptions[2].Item3);

            /*
             * Unsubscribe the same instrument again and receive error response.
             */
            FeedSource.Unsubscribe("l2205");

            Assert.AreEqual(4, FeedHandler.Subscriptions.Count);
            Assert.AreEqual("l2205", FeedHandler.Subscriptions[3].Item1);
            Assert.AreNotEqual(0, FeedHandler.Subscriptions[3].Item2.Code);
            Assert.IsFalse(FeedHandler.Subscriptions[3].Item3);
        }

        [TestMethod("Subscribe and receive OHLC.")]
        public void ReceiveOHLC()
        {
            /*
             * Subscribe for instrument and receive OHLC.
             */
            new SimulatedConfigurator(OHLCs, Instruments).Configure(out var broker, out var feedSource);

            Broker = (SimulatedBroker)broker;
            Broker.Register(OrderHandler, BrokerExchange);

            FeedSource = (SimulatedFeedSource)feedSource;
            FeedSource.Register(FeedHandler, FeedSourceExchange);
            FeedSource.Subscribe("l2205");

            while (FeedSource.Flop()) ;

            /*
             * Check feed handler receives only l2205 OHLC.
             */
            Assert.AreEqual(1, FeedHandler.OHLCs.Count);
            Assert.AreEqual("l2205", FeedHandler.OHLCs.First().InstrumentID);
        }

        [TestMethod("Trade orders.")]
        public void TradeOrders()
        {
            new SimulatedConfigurator(Ticks, Instruments).Configure(out var broker, out var feedSource);

            Broker = (SimulatedBroker)broker;
            Broker.Register(OrderHandler, BrokerExchange);

            FeedSource = (SimulatedFeedSource)feedSource;
            FeedSource.Register(FeedHandler, FeedSourceExchange);

            /*
             * Request two orders, one is completed at one trade, and the other is completed by two trades.
             * 
             * 1. Request two trades.
             * 2. Mock feeds and have the first order complete trade.
             * 3. Mock feeds and have the second order trade a part.
             * 4. Mock feeds and have the second order compelte trade.
             */

            Broker.New(new NewOrder
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                OrderID = Broker.NewOrderID,
                Price = 8899,
                Quantity = 5,
                Direction = Direction.Buy,
                Offset = Offset.Open
            });

            Broker.New(new NewOrder
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                OrderID = Broker.NewOrderID,
                Price = 8898,
                Quantity = 15,
                Direction = Direction.Buy,
                Offset = Offset.Open
            });

            /*
             * Send first two instrument status updates.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.IsTrue(FeedSource.Flop());

            /*
             * 1. Send the first tick, no order trade.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.AreEqual(0, OrderHandler.Trades.Count);

            /*
             * 2. Send the second tick, the first order is completed, and the
             *    second order is not traded.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.AreEqual(1, OrderHandler.Trades.Count);

            var trade = OrderHandler.Trades[0].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual(8899, trade.TradePrice);
            Assert.AreEqual(5, trade.Quantity);
            Assert.AreEqual(5, trade.TradeQuantity);
            Assert.AreEqual(0, trade.LeaveQuantity);
            Assert.AreEqual(OrderStatus.Completed, trade.Status);

            /*
             * 3. Send the third tick and the second order trades a portion
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.AreEqual(2, OrderHandler.Trades.Count);

            trade = OrderHandler.Trades[1].Item1; ;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual(8898, trade.TradePrice);
            Assert.AreEqual(15, trade.Quantity);
            Assert.AreEqual(10, trade.TradeQuantity);
            Assert.AreEqual(5, trade.LeaveQuantity);
            Assert.AreEqual(OrderStatus.Trading, trade.Status);

            /*
             * 4. Send the last tick and the second order is completed.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.AreEqual(3, OrderHandler.Trades.Count);

            trade = OrderHandler.Trades[2].Item1; ;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual(8898, trade.TradePrice);
            Assert.AreEqual(15, trade.Quantity);
            Assert.AreEqual(5, trade.TradeQuantity);
            Assert.AreEqual(0, trade.LeaveQuantity);
            Assert.AreEqual(OrderStatus.Completed, trade.Status);
        }

        [TestMethod("Request but delete orders.")]
        public void DeleteOrders()
        {
            new SimulatedConfigurator(Ticks, Instruments).Configure(out var broker, out var feedSource);

            Broker = (SimulatedBroker)broker;
            Broker.Register(OrderHandler, BrokerExchange);

            FeedSource = (SimulatedFeedSource)feedSource;
            FeedSource.Register(FeedHandler, FeedSourceExchange);

            /*
             * Request two orders, one is untouched, and the other is proportionally traded.
             * And the two orders are deleted.
             */

            var orderID1 = Broker.NewOrderID;
            var orderID2 = Broker.NewOrderID;

            Broker.New(new NewOrder
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                OrderID = orderID1,
                Price = 8899,
                Quantity = 5,
                Direction = Direction.Buy,
                Offset = Offset.Open
            });

            Broker.New(new NewOrder
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                OrderID = orderID2,
                Price = 8898,
                Quantity = 15,
                Direction = Direction.Buy,
                Offset = Offset.Open
            });

            /*
             * Send first two instrument status updates.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.IsTrue(FeedSource.Flop());

            /*
             * Send the first tick, no order trade.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.AreEqual(0, OrderHandler.Trades.Count);

            /*
             * 1. Delete the first order.
             */
            Broker.Delete(new DeleteOrder { OrderID = orderID1, InstrumentID = "l2205" });

            /*
             * Trigger the broker deleting order.
             * Handler receives the first trade with Deleted status.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.AreEqual(1, OrderHandler.Trades.Count);

            var trade = OrderHandler.Trades[0].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual(orderID1, trade.OrderID);
            Assert.AreEqual(5, trade.Quantity);
            Assert.AreEqual(8899, trade.Price);
            Assert.AreEqual(double.MaxValue, trade.TradePrice);
            Assert.AreEqual(0, trade.TradeQuantity);
            Assert.AreEqual(5, trade.LeaveQuantity);
            Assert.AreEqual(OrderStatus.Deleted, trade.Status);

            /*
             * 2. Send the following ticks and it trades a proportion.
             * 
             * Now the handler should receive the second trade response.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.AreEqual(2, OrderHandler.Trades.Count);

            /*
             * 3. Delete the order.
             */
            Broker.Delete(new DeleteOrder { OrderID = orderID2, InstrumentID = "l2205" });

            /*
             * Trigger deletion.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.AreEqual(3, OrderHandler.Trades.Count);

            trade = OrderHandler.Trades[2].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual(orderID2, trade.OrderID);
            Assert.AreEqual(15, trade.Quantity);
            Assert.AreEqual(8898, trade.Price);
            Assert.AreEqual(double.MaxValue, trade.TradePrice);
            Assert.AreEqual(0, trade.TradeQuantity);
            Assert.AreEqual(5, trade.LeaveQuantity);
            Assert.AreEqual(OrderStatus.Deleted, trade.Status);

            /*
             * 4. Delete a non existing order.
             * 
             * Delete the second order again.
             */
            Broker.Delete(new DeleteOrder { OrderID = orderID2, InstrumentID = "l2205" });

            /*
             * Trigger deletion.
             */
            Assert.IsTrue(FeedSource.Flop());
            Assert.AreEqual(4, OrderHandler.Trades.Count);

            trade = OrderHandler.Trades[3].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual(orderID2, trade.OrderID);
            Assert.AreEqual(int.MaxValue, trade.Quantity);
            Assert.AreEqual(double.MaxValue, trade.Price);
            Assert.AreEqual(double.MaxValue, trade.TradePrice);
            Assert.AreEqual(int.MaxValue, trade.TradeQuantity);
            Assert.AreEqual(int.MaxValue, trade.LeaveQuantity);
            Assert.AreEqual(OrderStatus.None, trade.Status);

            var description = OrderHandler.Trades[3].Item2;

            Assert.AreNotEqual(0, description.Code);
        }
    }

    internal class MockedExchange : IExchangeListener
    {
        public void OnConnected(bool isConnected)
        {
            Connected = isConnected;
        }

        internal bool Connected { get; private set; } = false;
    }

    internal class MockedFeedHandler : IFeedHandler
    {
        public void OnFeed(Tick tick)
        {
            Ticks.Add(tick);
        }

        public void OnFeed(OHLC ohlc)
        {
            OHLCs.Add(ohlc);
        }

        public void OnFeed(Instrument instrument)
        {
            Instruments.Add(instrument);
        }

        public void OnSubscribed(string instrumentID, Description description, bool subscribed)
        {
            Subscriptions.Add((instrumentID, description, subscribed));
        }

        internal List<Instrument> Instruments { get; init; } = new List<Instrument>();
        internal List<Tick> Ticks { get; init; } = new List<Tick>();
        internal List<OHLC> OHLCs { get; init; } = new List<OHLC>();
        internal List<(string, Description, bool)> Subscriptions = new List<(string, Description, bool)>();
    }

    internal class MockedOrderHandler : IOrderHandler
    {
        public void OnTrade(Trade trade, Description description)
        {
            Trades.Add((trade, description));
        }

        internal List<(Trade, Description)> Trades { get; init; } = new List<(Trade, Description)>();
    }
}
