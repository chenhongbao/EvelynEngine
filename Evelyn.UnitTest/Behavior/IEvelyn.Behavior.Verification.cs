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
using System.Linq;

namespace Evelyn.UnitTest.Behavior
{
    [TestClass]
    public class EvelynBehaviorVerfication : DataInitialize
    {
        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();
        }

        [TestMethod("Run feed source for client service.")]
        public void RunFeedSourceForClientService()
        {
            IEvelyn engine = IEvelyn.NewInstance;

            var mockedClientService = new MockedClientService();
            var mockedConfiguator = new MockedConfigurator();

            engine.RegisterRemoteClient(mockedClientService)
                .Configure(mockedConfiguator);

            mockedConfiguator.FeedSource.MockedConnect(true);

            /*
             * Engine forwards market data from feed source to client.
             * 
             * 1. Client subscribes for the specified instrument.
             * 2. Feed source receives market data.
             * 3. Engine selects the feeds of the subscribed instruments and forwards them to client.
             * 4. Client unsubscribes the instrument, and feed source sends unsubscription response.
             * 5. Feed source sends market data and client receives no market data any more.
             */

            var client = mockedClientService.GetClientOrCreate("MOCKED_CLIENT");

            /*
             * 1. Client subscribes for instrument and feed source replies OK.
             */
            mockedClientService.MockedSubscribe("l2205", true, "MOCKED_CLIENT");

            /*
             * Feed source receives the request.
             */
            Assert.AreEqual(1, mockedConfiguator.FeedSource.SubscribedInstruments.Count);
            Assert.AreEqual("l2205", mockedConfiguator.FeedSource.SubscribedInstruments[0]);

            /*
             * Feed source sends subscription response.
             */
            mockedConfiguator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK" }, true);

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
             * 4. Unsubscribe instrument.
             */
            mockedClientService.MockedSubscribe("l2205", false, "MOCKED_CLIENT");

            /*
             * Feed source receives unsubscription request.
             */
            Assert.AreEqual(1, mockedConfiguator.FeedSource.UnsubscribedInstruments.Count);
            Assert.AreEqual("l2205", mockedConfiguator.FeedSource.UnsubscribedInstruments[0]);

            mockedConfiguator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK" }, false);

            /*
             * Client receives unsubscription response.
             */
            Assert.AreEqual("l2205", client.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, client.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK", client.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(false, client.ReceivedSubscribe.Item3);

            /*
             * 5. Feed source sends market data and client receives no data.
             */
            client.ReceivedTicks.Clear();
            client.ReceivedOHLCs.Clear();
            client.ReceivedInstruments.Clear();

            MockedTicks.ForEach(tick => mockedConfiguator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => mockedConfiguator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => mockedConfiguator.FeedSource.MockedReceive(instrument));

            Assert.AreEqual(0, client.ReceivedTicks.Count);
            Assert.AreEqual(0, client.ReceivedOHLCs.Count);
            Assert.AreEqual(0, client.ReceivedInstruments.Count);
        }

        [TestMethod("Un/Subscribe instrument more than once.")]
        public void DuplicatedSubscription()
        {
            IEvelyn engine = IEvelyn.NewInstance;

            var mockedClientService = new MockedClientService();
            var mockedConfiguator = new MockedConfigurator();

            engine.RegisterRemoteClient(mockedClientService)
                .Configure(mockedConfiguator);

            mockedConfiguator.FeedSource.MockedConnect(true);

            /*
             * Over unsubscribe an instrument and receive error response.
             * 
             * 1. Client subscribes for the specified instrument and feed source sends subscription response.
             * 2. Client subscribes again, feed source doesn't receive new request, and engine sends the error response.
             * 3. Client unsubscribes the instrument and feed source sends the unsubscription response.
             * 4. Client unsubscribes the instrument again, feed source receives no request, and engine sends the error response.
             */

            var client = mockedClientService.GetClientOrCreate("MOCKED_CLIENT");

            /*
             * 1. Client subscribes for instrument and feed source replies OK.
             */
            mockedClientService.MockedSubscribe("l2205", true, "MOCKED_CLIENT");

            /*
             * Feed source receives the request.
             */
            Assert.AreEqual(1, mockedConfiguator.FeedSource.SubscribedInstruments.Count);
            Assert.AreEqual("l2205", mockedConfiguator.FeedSource.SubscribedInstruments[0]);

            /*
             * Feed source sends subscription response.
             */
            mockedConfiguator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK" }, true);

            Assert.AreEqual("l2205", client.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, client.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK", client.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(true, client.ReceivedSubscribe.Item3);

            /*
             * 2. Client subscribes again, feed source doesn't receive request, and engine sends the error response.
             */
            mockedClientService.MockedSubscribe("l2205", true, "MOCKED_CLIENT");

            /*
             * Feed source doesn't receive new request.
             */
            Assert.AreEqual(1, mockedConfiguator.FeedSource.SubscribedInstruments.Count);

            /*
             * Engine sends the error response.
             */
            Assert.AreEqual("l2205", client.ReceivedSubscribe.Item1);
            Assert.AreNotEqual(0, client.ReceivedSubscribe.Item2.Code);
            Assert.AreNotEqual("OK", client.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(true, client.ReceivedSubscribe.Item3);

            /*
             * 3. Client unsubscribes the instrument and feed source sends the unsubscription response.
             */
            mockedClientService.MockedSubscribe("l2205", false, "MOCKED_CLIENT");

            /*
             * Feed source receives unsubscription request.
             */
            Assert.AreEqual(1, mockedConfiguator.FeedSource.UnsubscribedInstruments.Count);
            Assert.AreEqual("l2205", mockedConfiguator.FeedSource.UnsubscribedInstruments[0]);

            mockedConfiguator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK" }, false);

            /*
             * Client receives unsubscription response.
             */
            Assert.AreEqual("l2205", client.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, client.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK", client.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(false, client.ReceivedSubscribe.Item3);

            /*
             * 4. Client unsubscribes the same instrument again, feed source doesnt' receive the unsubscription request,
             * and engine sends the error response.
             */

            mockedClientService.MockedSubscribe("l2205", false, "MOCKED_CLIENT");

            /*
             * Feed source doesn't receive new unsubscription request.
             */
            Assert.AreEqual(1, mockedConfiguator.FeedSource.UnsubscribedInstruments.Count);

            /*
             * Engine sends the error response to client.
             */
            Assert.AreEqual("l2205", client.ReceivedSubscribe.Item1);
            Assert.AreNotEqual(0, client.ReceivedSubscribe.Item2.Code);
            Assert.AreNotEqual("OK", client.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(false, client.ReceivedSubscribe.Item3);
        }

        [TestMethod("Run order for client service.")]
        public void RunOrderForClientService()
        {
            var baseTime = DateTime.Now;
            var baseDay = DateOnly.FromDateTime(baseTime);

            IEvelyn engine = IEvelyn.NewInstance;

            var mockedClientService = new MockedClientService();
            var mockedConfigurator = new MockedConfigurator();

            engine.RegisterRemoteClient(mockedClientService)
                .RegisterInstrument(
                new Instrument
                {
                    InstrumentID = "l2205",
                    TradingDay = baseDay,
                    Status = InstrumentStatus.Continous,
                    EnterTime = baseTime
                },
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = baseDay,
                    Status = InstrumentStatus.Continous,
                    EnterTime = baseTime
                })
                .Configure(mockedConfigurator);

            mockedConfigurator.Broker.MockedConnect(true);

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

            var client = mockedClientService.GetClientOrCreate("MOCKED_CLIENT");
            var fakeClient = mockedClientService.GetClientOrCreate("MOCKED_CLIENT_FAKE");

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
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
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
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                },
                "MOCKED_CLIENT_FAKE");

            var newOrder = mockedConfigurator.Broker.ReceivedNewOrders[0];

            /*
             * Engine rewrites the order ID, don't compare the order ID on broker side.
             */
            Assert.AreEqual("l2205", newOrder.InstrumentID);
            Assert.AreEqual(8888, newOrder.Price);
            Assert.AreEqual(2, newOrder.Quantity);

            var newOrderFake = mockedConfigurator.Broker.ReceivedNewOrders[1];

            Assert.AreEqual("l2205", newOrderFake.InstrumentID);
            Assert.AreEqual(7777, newOrderFake.Price);
            Assert.AreEqual(3, newOrderFake.Quantity);

            /*
             * 2. Mocked broker trades 1 quantity of the given order and returns trade, and engine forwards the response to corresponding client.
             */
            mockedConfigurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = newOrder.OrderID, /* Engine rewrites the order ID. */
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 8890,
                    TradeQuantity = 1,
                    LeaveQuantity = 1,
                    Status = OrderStatus.Trading,
                    Message = "Trading"
                },
                new Description
                {
                    Code = 0,
                    Message = "Order is trading."
                });

            /*
             * Client receives trade response.
             * 
             * Engine rewrites the order ID that presents to broker, and writes back the original
             * order ID when sends trade back to client.
             */
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
            mockedClientService.MockedDelete(
                new DeleteOrder
                {
                    OrderID = "MOCKED_ORDER_1",
                    InstrumentID = "l2205"
                },
                "MOCKED_CLIENT");

            Assert.AreEqual(newOrder.OrderID, mockedConfigurator.Broker.ReceivedDeleteOrders[0]);

            mockedConfigurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = newOrder.OrderID, /* Engine rewrites the order ID. */
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_2",
                    TradePrice = 0, /* no actual trade happens, so price and volume are 0*/
                    TradeQuantity = 0,
                    LeaveQuantity = 1,
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
            Assert.AreEqual(0, fakeClient.ReceivedTrades.Count);
        }

        [TestMethod("Call OHLC generator.")]
        public void CallOHLCGenerator()
        {
            IEvelyn engine = IEvelyn.NewInstance;

            var mockedOHLCGenerator = new MockedOHLCGenerator();
            var mockedClientService = new MockedClientService();
            var mockedConfiguator = new MockedConfigurator();

            engine.GenerateOHLC(mockedOHLCGenerator)
                .RegisterRemoteClient(mockedClientService)
                .Configure(mockedConfiguator);

            mockedConfiguator.FeedSource.MockedConnect(true);

            /*
             * Engine calls customized OHCLGenerator to generate OHLC data. When the interface returns null, no OHLC is available, or returns an OHLC instance.
             * 
             * 1. Mocked generator returns an OHLC at the first Tick.
             * 2. Mocked generator returns null after the first OHLC.
             * 
             * The mocked client subscribes for the specified instrument so it will received the OHLC.
             */
            var client = mockedClientService.GetClientOrCreate("MOCKED_CLIENT");

            mockedClientService.MockedSubscribe("l2205", true, "MOCKED_CLIENT");
            mockedConfiguator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK" }, true);

            /*
             * Here sends the ticks.
             */
            MockedTicks.ForEach(tick => mockedConfiguator.FeedSource.MockedReceive(tick));

            Assert.AreEqual(1, client.ReceivedOHLCs.Count);
            Assert.AreEqual(mockedOHLCGenerator.GeneratedOHLC, client.ReceivedOHLCs[0]);
        }

        [TestMethod("Run feed source on local client.")]
        public void RunFeedSourceForLocalClient()
        {
            IEvelyn engine = IEvelyn.NewInstance;

            var mockedClient = new MockedLocalClient();
            var mockedClientFake = new MockedLocalClient();
            var mockedClientService = new MockedClientService();
            var mockedConfigurator = new MockedConfigurator();

            engine.RegisterRemoteClient(mockedClientService)
                .RegisterLocalClient("MockedClient", mockedClient, "l2205")
                .RegisterLocalClient("MockedClientFake", mockedClientFake, "pp2205")
                .Configure(mockedConfigurator);

            mockedConfigurator.FeedSource.MockedConnect(true);

            /*
             * Engine tranfers messages between broker and client.
             * 
             * Client mocks requests to engine, and broker and feed source mocks market data to client.
             * Engine assigns a unique client ID to each client internally.
             */

            /*
             * 1. Client subscribes for instrument.
             * 
             * The client doesn't explicitly subscribe an instrument. When the client is registered with
             * the engine, subscribed instrument ID is passed to engine as another parameter.
             * 
             * So here just check the feed source receives the subscription request.
             */
            Assert.AreEqual(2, mockedConfigurator.FeedSource.SubscribedInstruments.Count);
            Assert.IsTrue(mockedConfigurator.FeedSource.SubscribedInstruments.Contains("l2205"));
            Assert.IsTrue(mockedConfigurator.FeedSource.SubscribedInstruments.Contains("pp2205"));

            /*
             * Feed source sends the subscription responses.
             */
            mockedConfigurator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK-l2205" }, true);
            mockedConfigurator.FeedSource.MockedReplySubscribe("pp2205", new Description { Code = 0, Message = "OK-pp2205" }, true);

            /*
             * And also client receives subscription response.
             */
            Assert.AreEqual("l2205", mockedClient.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, mockedClient.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK-l2205", mockedClient.ReceivedSubscribe.Item2.Message);

            Assert.AreEqual("pp2205", mockedClientFake.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, mockedClientFake.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK-pp2205", mockedClientFake.ReceivedSubscribe.Item2.Message);

            /*
             * 2. Client receives market data.
             * 
             * Just check any of the clients receives correct market data. Again, client receives market
             * data in the order they are sent.
             */
            MockedTicks.ForEach(tick => mockedConfigurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => mockedConfigurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => mockedConfigurator.FeedSource.MockedReceive(instrument));

            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), mockedClient.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), mockedClient.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), mockedClient.ReceivedInstruments);

            /*
             * 3. Change subscribed instruments by calling AlterLocalClient method.
             * 
             * Just change the subscribed instruments, then extra instruments are requested, and missing instruments are unsubscribed.
             */
            engine.AlterClient("MockedClient", "pp2205");

            /*
             * Because the instrument is already subscribed, no more feed source subscription.
             */
            Assert.AreEqual(2, mockedConfigurator.FeedSource.SubscribedInstruments.Count);

            var i0 = mockedConfigurator.FeedSource.SubscribedInstruments[0];
            var i1 = mockedConfigurator.FeedSource.SubscribedInstruments[1];

            Assert.IsTrue((i0 == "l2205" && i1 == "pp2205") || (i1 == "l2205" && i0 == "pp2205"));

            /*
             * Feed source doesn't receive the subscription request for pp2205, but engine sends a subscription response.
             */
            Assert.AreEqual("pp2205", mockedClient.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, mockedClient.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual(String.Empty, mockedClient.ReceivedSubscribe.Item2.Message);

            /*
             * Check feed source receives unsubscription request.
             */
            Assert.AreEqual(1, mockedConfigurator.FeedSource.UnsubscribedInstruments.Count);
            Assert.AreEqual("l2205", mockedConfigurator.FeedSource.UnsubscribedInstruments[0]);

            /*
             * Feed source sends the unsubscription response.
             */
            mockedConfigurator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK-l2205" }, false);

            /*
             * Client receives unsubscription response.
             */
            Assert.AreEqual("l2205", mockedClient.ReceivedUnsubscribe.Item1);
            Assert.AreEqual(0, mockedClient.ReceivedUnsubscribe.Item2.Code);
            Assert.AreEqual("OK-l2205", mockedClient.ReceivedUnsubscribe.Item2.Message);

            /*
             * Feed source sends market data again, and client shall receives only newly subscribed data.
             */
            mockedClient.ReceivedTicks.Clear();
            mockedClient.ReceivedOHLCs.Clear();
            mockedClient.ReceivedInstruments.Clear();

            MockedTicks.ForEach(tick => mockedConfigurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => mockedConfigurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => mockedConfigurator.FeedSource.MockedReceive(instrument));

            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "pp2205").ToList(), mockedClient.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "pp2205").ToList(), mockedClient.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "pp2205").ToList(), mockedClient.ReceivedInstruments);
        }

        [TestMethod("Run order on local client.")]
        public void RunOrderForLocalClient()
        {
            var baseTime = DateTime.Now;
            var baseDay = DateOnly.FromDateTime(baseTime);

            IEvelyn engine = IEvelyn.NewInstance;

            var mockedClient = new MockedLocalClient();
            var mockedClientFake = new MockedLocalClient();
            var mockedConfiguator = new MockedConfigurator();

            /*
             * Prepare engine for ordering.
             */
            engine.RegisterLocalClient("MockedClient", mockedClient, "l2205")
                .RegisterLocalClient("MockedClientFake", mockedClientFake, "pp2205")
                .RegisterInstrument(
                new Instrument
                {
                    InstrumentID = "l2205",
                    TradingDay = baseDay,
                    Status = InstrumentStatus.Continous,
                    EnterTime = baseTime
                },
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = baseDay,
                    Status = InstrumentStatus.Continous,
                    EnterTime = baseTime
                })
                .Configure(mockedConfiguator);

            mockedConfiguator.Broker.MockedConnect(true);

            /*
             * Client requests new order, order is partly traded, and then client deletes the order.
             * 
             * 1. Client request new order and mocked broker receives the new order request.
             * 2. Mocked broker sends a trade and client receives the trade response.
             * 3. Client deletes the existing order and mocked broker receives deletion request, deletes the order and sends update with deletion status.
             * 4. Client receives the deletion update.
             * 
             * The fake client receives no responses on the order.
             */

            /*
             * 1. Clients request two orders.
             */
            mockedClient.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                });

            /*
             * Let fake client requests another new order.
             */
            mockedClientFake.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_FAKE_1",
                    Price = 7777,
                    Quantity = 3,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                });

            /*
             * Check broker receives two order requests.
             * 
             * Engine rewrites the order ID presented to broker, so don't check
             * the broker's order ID.
             */
            var newOrder = mockedConfiguator.Broker.ReceivedNewOrders[0];

            Assert.AreEqual("l2205", newOrder.InstrumentID);
            Assert.AreEqual(8888, newOrder.Price);
            Assert.AreEqual(2, newOrder.Quantity);

            var newOrderFake = mockedConfiguator.Broker.ReceivedNewOrders[1];

            Assert.AreEqual("l2205", newOrderFake.InstrumentID);
            Assert.AreEqual(7777, newOrderFake.Price);
            Assert.AreEqual(3, newOrderFake.Quantity);

            /*
             * 2. Broker sends a trade response.
             */
            mockedConfiguator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = newOrder.OrderID, /* Engine rewrites order ID. */
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 8890,
                    TradeQuantity = 1,
                    LeaveQuantity = 1,
                    Status = OrderStatus.Trading,
                    Message = "Trading"
                },
                new Description
                {
                    Code = 0,
                    Message = "Order is trading."
                });

            /*
             * Client receives the trade response.
             */
            var trade = mockedClient.ReceivedTrades[0].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", trade.OrderID);
            Assert.AreEqual("MOCKED_ORDER_1_TRADE_1", trade.TradeID);
            Assert.AreEqual(2, trade.Quantity);
            Assert.AreEqual(1, trade.TradeQuantity);
            Assert.AreEqual(1, trade.LeaveQuantity);
            Assert.AreEqual(8890, trade.TradePrice);
            Assert.AreEqual(OrderStatus.Trading, trade.Status);

            var description = mockedClient.ReceivedTrades[0].Item2;

            Assert.AreEqual(0, description.Code);
            Assert.AreEqual("Order is trading.", description.Message);

            /*
             * 3. Client sends a deletion request, broker receives the request and deletes the requested order, and sends
             * back a trade update with deletion status.
             */

            mockedClient.MockedDelete(new DeleteOrder
            {
                OrderID = "MOCKED_ORDER_1",
                InstrumentID = "l2205"
            });

            Assert.AreEqual(newOrder.OrderID, mockedConfiguator.Broker.ReceivedDeleteOrders[0]);

            /*
             * Mocked broker sends a trade update with deletion status.
             */
            mockedConfiguator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = newOrder.OrderID, /* Engine rewrites order ID. */
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_2",
                    TradePrice = 0, /* no actual trade happens, so price and volume are 0*/
                    TradeQuantity = 0,
                    LeaveQuantity = 1,
                    Status = OrderStatus.Deleted, /* status is deleted */
                    Message = "Deleted"
                },
                new Description
                {
                    Code = 1,
                    Message = "Order is deleted."
                });

            /*
             * 4. Client receives the deletion update.
             */
            trade = mockedClient.ReceivedTrades[1].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", trade.OrderID);
            Assert.AreEqual("MOCKED_ORDER_1_TRADE_2", trade.TradeID);
            Assert.AreEqual(0, trade.TradePrice);
            Assert.AreEqual(0, trade.TradeQuantity);
            Assert.AreEqual(OrderStatus.Deleted, trade.Status);

            description = mockedClient.ReceivedTrades[1].Item2;

            Assert.AreEqual(1, description.Code);
            Assert.AreEqual("Order is deleted.", description.Message);

            /*
             * Check the fake client receives no trade.
             */
            Assert.AreEqual(0, mockedClientFake.ReceivedTrades.Count);
        }
    }
}
