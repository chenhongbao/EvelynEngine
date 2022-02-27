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
using Evelyn.UnitTest.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evelyn.UnitTest
{
    [TestClass]
    public class EvelynBehaviorVerfication
    {
        internal List<Tick> MockedTicks { get; private set; } = new List<Tick>();
        internal List<OHLC> MockedOHLCs { get; private set; } = new List<OHLC>();
        internal List<Instrument> MockedInstruments { get; private set; } = new List<Instrument>();

        [TestInitialize]
        public void Initialize()
        {
            /*
             * Initialize mocked ticks and OHLCs.
             */

            // TODO Initialize market data.
        }


        [TestMethod("IEvelyn calls IClientService's methods correctly.")]
        public void CallClientService()
        {
            IEvelyn engine = IEvelyn.New();

            var mockedClientService = new MockedClientService();
            var mockedConfiguator = new MockedConfigurator();

            engine.EnableRemoteClient(mockedClientService)
                .Configure(mockedConfiguator);

            /*
             * Engine forwards market data from feed source to client.
             * 
             * 1. Client subscribes for the specified instrument.
             * 2. Feed source receives market data.
             * 3. Engine selects the feeds of the subscribed instruments and forwards them to client.
             */

            /*
             * 1. Client subscribes for instrument and feed source replies OK.
             */
            mockedClientService.MockedSubscribe("l2205", true, "MOCKED_CLIENT");
            mockedConfiguator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK" }, true, "MOCKED_CLIENT");

            var client = mockedClientService.GetClient("MOCKED_CLIENT");

            Assert.AreEqual("l2205", client.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, client.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK", client.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(true, client.ReceivedSubscribe.Item3);

            /*
             * 2. Feed source receives market data and input them into engine. Engine forwards the market data accordingly.
             */
            MockedTicks.ForEach(tick => mockedConfiguator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => mockedConfiguator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => mockedConfiguator.FeedSource.MockedReceive(instrument));

            /*
             * 3. Check receiving correct market data.
             */
            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), client.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), client.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), client.ReceivedInstruments);

            /*
             * Engine receives order request from client, route the order to broker, then forward the responses from broker to client.
             * 
             * 1. Client requests new order.
             * 2. Engine routes new order to broker.
             * 3. Broker trades a part of the given order and returns trades.
             * 4. Client requests deleting the new order.
             * 5. Engine routes the deleting request to broker.
             * 6. Broker deletes the unfinished order and returns trade with deletion state.
             */

            /*
             * 1. Client requests a new order with specified client ID, and engine routes the order to broker.
             */
            mockedClientService.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = OrderDirection.Buy,
                    Offset = OrderOffset.Open,
                },
                "MOCKED_CLIENT");

            /* 
             * An extra order is inserted so engine must select the correct client by ClientID.
             * Orders should arrive at broker in the order of their requesting.
             */
            mockedClientService.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_FAKE_1",
                    Price = 7777,
                    Quantity = 3,
                    Direction = OrderDirection.Buy,
                    Offset = OrderOffset.Open,
                },
                "MOCKED_CLIENT_FAKE");

            var newOrder = mockedConfiguator.Broker.ReceivedNewOrders[0];

            Assert.AreEqual("l2205", newOrder.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", newOrder.OrderID);
            Assert.AreEqual(8888, newOrder.Price);
            Assert.AreEqual(2, newOrder.Quantity);

            newOrder = mockedConfiguator.Broker.ReceivedNewOrders[1];

            Assert.AreEqual("l2205", newOrder.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_FAKE_1", newOrder.OrderID);
            Assert.AreEqual(7777, newOrder.Price);
            Assert.AreEqual(3, newOrder.Quantity);

            /*
             * 2. Mocked broker trades 1 quantity of the given order and returns trade, and engine forwards the response to corresponding client.
             */
            mockedConfiguator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = OrderDirection.Buy,
                    Offset = OrderOffset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 8890,
                    TradeQuantity = 1,
                    LeaveQuantity = 1,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Trading,
                    Message = "Trading"
                },
                new Description
                {
                    Code = 0,
                    Message = "Order is trading."
                });

            var trade = client.ReceivedTrades[0].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", trade.OrderID);
            Assert.AreEqual("MOCKED_ORDER_1_TRADE_1", trade.TradeID);
            Assert.AreEqual(2, trade.Quantity);
            Assert.AreEqual(1, trade.TradeQuantity);
            Assert.AreEqual(1, trade.LeaveQuantity);
            Assert.AreEqual(8890, trade.TradePrice);
            Assert.AreEqual(OrderStatus.Trading, trade.Status);

            var description = client.ReceivedTrades[0].Item2;

            Assert.AreEqual(0, description.Code);
            Assert.AreEqual("Order is trading.", description.Message);

            /*
             * 3. Client requests deleting an existing order, engine route this request to broker, and broker deletes the order and returns trade with deletion order state.
             */
            mockedClientService.MockedDelete("MOCKED_ORDER_1", "MOCKED_CLIENT");

            Assert.AreEqual("MOCKED_ORDER_1", client.ReceivedInstruments[0]);

            mockedConfiguator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = OrderDirection.Buy,
                    Offset = OrderOffset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_2",
                    TradePrice = 0, /* no actual trade happens, so price and volume are 0*/
                    TradeQuantity = 0,
                    LeaveQuantity = 1,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Deleted, /* status is deleted */
                    Message = "Deleted"
                },
                new Description
                {
                    Code = 1,
                    Message = "Order is deleted."
                });

            /*
             * For a deleted order, the last trade only reports the status change, not traded any volume.
             * So its trade price and trade volume are 0, and status is deleted.
             */

            trade = client.ReceivedTrades[1].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", trade.OrderID);
            Assert.AreEqual("MOCKED_ORDER_1_TRADE_2", trade.TradeID);
            Assert.AreEqual(0, trade.TradePrice);
            Assert.AreEqual(0, trade.TradeQuantity);
            Assert.AreEqual(OrderStatus.Deleted, trade.Status);

            description = client.ReceivedTrades[1].Item2;

            Assert.AreEqual(1, description.Code);
            Assert.AreEqual("Order is deleted.", description.Message);

            /*
             * Check the fake client receives no trade.
             */
            var fakeClient = mockedClientService.GetClient("MOCKED_CLIENT_FAKE");

            Assert.AreEqual(0, fakeClient.ReceivedTrades.Count);
        }

        private int CompareCollection<T>(ICollection<T> c0, ICollection<T> c1)
        {
            /*
             * Find the first different element in c1 compared to the element in c0 that has the same index.
             * If all elements are the same, returns int.MaxValue.
             */
            int index = -1;
            while (++index < c1.Count)
            {
                var e0 = c0.ElementAt(index) ?? throw new NoValueException("Left operand has no value at index " + index + ".");
                if (index >= c0.Count && !e0.Equals(c1.ElementAt(index)))
                {
                    return index;
                }
            }

            if (index < c0.Count)
            {
                return index;
            }
            else
            {
                return int.MaxValue;
            }
        }
    }

    internal static class MockedExtension
    {
        public static bool Equals(this Tick e0, Tick e1)
        {
            return e0.InstrumentID == e1.InstrumentID && e0.TimeStamp == e1.TimeStamp;
        }

        public static bool Equals(this OHLC e0, OHLC e1)
        {
            return e0.InstrumentID == e1.InstrumentID && e0.TimeStamp == e1.TimeStamp;
        }

        public static bool Equals(this Instrument e0, Instrument e1)
        {
            return e0.InstrumentID == e1.InstrumentID && e0.TimeStamp == e1.TimeStamp;
        }
    }
}
