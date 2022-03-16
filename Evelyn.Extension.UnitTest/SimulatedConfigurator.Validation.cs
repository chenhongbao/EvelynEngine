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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evelyn.Extension.UnitTest
{
    [TestClass]
    public class SimulatedConfiguratorValidation : SimulatedConfiguratorData
    {
        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();
        }

        [TestMethod("Subcribe and receive feeds.")]
        public void ReceiveFeeds()
        {
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
             * 1. Mock feeds, send the first instrument status update.
             */
            Assert.IsTrue(FeedSource.Flip());
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
            Assert.IsTrue(FeedSource.Flip());

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
                Assert.IsTrue(FeedSource.Flip());
            }

            /*
             * Check handler receives correct ticks.
             */
            var correctTicks = Ticks.Where(tick => tick.InstrumentID == "l2205").ToList();
            Assert.AreEqual(correctTicks.Count(), FeedHandler.Ticks.Count);

            for(int i = 0; i < FeedHandler.Ticks.Count; ++i)
            {
                Assert.AreEqual(correctTicks[i], FeedHandler.Ticks[i]);
            }

            /*
             * 3. Mock last two instrument status updates.
             */
            Assert.IsTrue(FeedSource.Flip());
            Assert.IsTrue(FeedSource.Flip());

            /*
             * Check handler receives the last two instrument statuses.
             */
            Assert.AreEqual(2, FeedHandler.Instruments.Count);
            Assert.AreEqual("l2205", FeedHandler.Instruments[1].InstrumentID);
            Assert.AreEqual(InstrumentStatus.Closed, FeedHandler.Instruments[1].Status);

            /*
             * All feeds are sent, exchange is disconnected.
             */
            Assert.IsFalse(FeedSource.Flip());
            Assert.IsFalse(FeedSourceExchange.Connected);

            /*
             * 4. Unsubscribe instrument.
             */
            FeedSource.Unsubscribe("l2205");

            Assert.AreEqual(2, FeedHandler.Subscriptions.Count);
            Assert.AreEqual("l2205", FeedHandler.Subscriptions[1].Item1);
            Assert.IsFalse(FeedHandler.Subscriptions[1].Item3);
        }

        [TestMethod("Trade orders.")]
        public void TradeOrders()
        {

        }

        [TestMethod("Request but delete orders.")]
        public void DeleteOrders()
        {

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
            throw new System.NotImplementedException();
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
