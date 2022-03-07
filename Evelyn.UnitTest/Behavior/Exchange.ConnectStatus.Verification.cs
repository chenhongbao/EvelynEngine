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

namespace Evelyn.UnitTest.Behavior
{
    [TestClass]
    public class ExchangeConnectStatusVerification : DataInitialize
    {
        internal IEvelyn Engine { get; private set; } = IEvelyn.NewInstance;
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal MockedLocalClient Client { get; private set; } = new MockedLocalClient();
        internal DateOnly TradingDay { get; private set; } = DateOnly.MaxValue;

        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();

            var baseTime = DateTime.Now;

            Engine = IEvelyn.NewInstance;
            TradingDay = DateOnly.FromDateTime(baseTime);

            Engine.RegisterLocalClient("MOCKED_CLIENT", Client, "l2205", "pp2205")
                .RegisterInstrument(
                 new Instrument
                 {
                     InstrumentID = "l2205",
                     TradingDay = TradingDay,
                     Status = InstrumentStatus.Continous,
                     EnterTime = baseTime
                 },
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.Closed,
                    EnterTime = baseTime
                })
                .Configure(Configurator);

            Configurator.Broker.MockedConnect(false);
        }

        [TestMethod("Subscribe instruments when exchange is connected.")]
        public void SubscribeWhenConnected()
        {
            /*
             * Subscribe instruments when exchange is disconnected, the instruments
             * are kept by feed source. When exchange is connected again, engine
             * sends out those pending subscription requests.
             * 
             * 1. Exchange is disconnected.
             * 2. Client subscribes for instruments, but all blocked by engin, feed source
             *    receives no request.
             * 3. Exchange is connected, engine sends those pending requests to feed source.
             * 4. Feed source sends market data and client receives the data.
             */

            /*
             * The initial exchange state is all disconnected, and configure the engine
             * would also subscribe for the given instruments.
             * 
             * So here just check mocked feed source receives no request.
             */
            Assert.AreEqual(0, Configurator.FeedSource.SubscribedInstruments.Count);

            /*
             * Check client receives the initial instruments.
             */
            Assert.AreEqual(2, Client.ReceivedInstruments.Count);

            var ins0 = Client.ReceivedInstruments[0].InstrumentID;
            var ins1 = Client.ReceivedInstruments[1].InstrumentID;

            Assert.IsTrue((ins0 == "l2205" && ins1 == "pp2205") || (ins1 == "l2205" && ins0 == "pp2205"));

            /*
             * Exchange is connected.
             */
            Configurator.FeedSource.MockedConnect(true);

            /*
             * Check feed source receives susbcription requests for the given instruments.
             */
            Assert.AreEqual(02, Configurator.FeedSource.SubscribedInstruments.Count);

            var i0 = Configurator.FeedSource.SubscribedInstruments[0];
            var i1 = Configurator.FeedSource.SubscribedInstruments[1];

            Assert.IsTrue((i0 == "l2205" && i1 == "pp2205") || (i1 == "l2205" && i0 == "pp2205"));

            /*
             * Feed source sends back the subscription responses and clients receive the respones.
             */
            Configurator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK-l2205" }, true);

            Assert.AreEqual("l2205", Client.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, Client.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK-l2205", Client.ReceivedSubscribe.Item2.Message);

            Configurator.FeedSource.MockedReplySubscribe("pp2205", new Description { Code = 0, Message = "OK-pp2205" }, true);

            Assert.AreEqual("pp2205", Client.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, Client.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK-pp2205", Client.ReceivedSubscribe.Item2.Message);

            /*
             * Feed source sends market data and client receives the data.
             */
            Client.ReceivedInstruments.Clear();

            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => Configurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => Configurator.FeedSource.MockedReceive(instrument));

            CompareCollection(MockedTicks, Client.ReceivedTicks);
            CompareCollection(MockedOHLCs, Client.ReceivedOHLCs);
            CompareCollection(MockedInstruments, Client.ReceivedInstruments);
        }

        [TestMethod("Reject new order when exchange is disconnected.")]
        public void RejectNewOrderWhenDisconnected()
        {
            /*
             * Exchange is disconnected, and engine blocks all orders from clients and
             * return error response.
             */

            Client.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "pp2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8555,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                });

            /*
             * Broker receives no order request.
             */
            Assert.AreEqual(0, Configurator.Broker.ReceivedNewOrders.Count);

            /*
             * Client receives error response sent by engine.
             */
            Assert.AreEqual(1, Client.ReceivedTrades.Count);

            var trade = Client.ReceivedTrades[0].Item1;

            Assert.AreEqual("pp2205", trade.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", trade.OrderID);
            Assert.AreEqual(8555, trade.Price);
            Assert.AreEqual(2, trade.Quantity);
            Assert.AreEqual(Direction.Buy, trade.Direction);
            Assert.AreEqual(Offset.Open, trade.Offset);
            Assert.AreEqual(OrderStatus.Rejected, trade.Status);

            var description = Client.ReceivedTrades[0].Item2;

            Assert.AreNotEqual(0, description.Code);
        }

        [TestMethod("Reject deleting order when exchange is disconnected.")]
        public void RejectDeleteOrderWhenDisconnected()
        {
            /*
             * Exchange is disconnected, and engine blocks all deleting request from clients and
             * return error response.
             */

            Client.MockedDelete(new DeleteOrder
            {
                OrderID = "ANY_ID",
                InstrumentID = "ANY_INSTRUMENT_ID"
            });

            /*
             * Broker receives no order request.
             */
            Assert.AreEqual(0, Configurator.Broker.ReceivedNewOrders.Count);

            /*
             * Client receives error response sent by engine.
             */
            Assert.AreEqual(1, Client.ReceivedTrades.Count);

            var trade = Client.ReceivedTrades[0].Item1;

            Assert.AreEqual("ANY_INSTRUMENT_ID", trade.InstrumentID);
            Assert.AreEqual("ANY_ID", trade.OrderID);
            Assert.AreEqual(double.MaxValue, trade.Price);
            Assert.AreEqual(int.MaxValue, trade.Quantity);
            Assert.AreEqual(double.MaxValue, trade.TradePrice);
            Assert.AreEqual(int.MaxValue, trade.TradeQuantity);
            Assert.AreEqual(OrderStatus.Deleted, trade.Status);

            var description = Client.ReceivedTrades[0].Item2;

            Assert.AreNotEqual(0, description.Code);
        }
    }
}
