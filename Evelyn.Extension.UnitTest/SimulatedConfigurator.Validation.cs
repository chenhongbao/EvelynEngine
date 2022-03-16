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
    public class SimulatedConfiguratorValidation
    {
        internal List<Tick> Ticks { get; init; } = new List<Tick>();
        internal List<Instrument> Instruments { get; init; } = new List<Instrument>();
        internal MockedExchange BrokerExchange { get; private set; } = new MockedExchange();
        internal MockedExchange FeedSourceExchange { get; private set; } = new MockedExchange();
        internal MockedOrderHandler OrderHandler { get; private set; } = new MockedOrderHandler();
        internal MockedFeedHandler FeedHandler { get; private set; } = new MockedFeedHandler();
        internal IConfigurator Configurator { get; private set; } = new SimulatedConfigurator(new List<Tick> { }, new List<Instrument> { });
        internal DateTime BaseTime { get; init; } = DateTime.Now;
        internal DateOnly TradingDay { get; init; } = DateOnly.FromDateTime(DateTime.Now);

        [TestInitialize]
        public void Initialize()
        {
            /*
             * Reset data.
             */
            BrokerExchange = new MockedExchange();
            FeedSourceExchange = new MockedExchange();
            OrderHandler = new MockedOrderHandler();
            FeedHandler = new MockedFeedHandler();

            Instruments.Clear();
            Ticks.Clear();

            /*
             * Two instruments.
             */
            Instruments.Add(new Instrument { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, Status = InstrumentStatus.Continous, EnterTime = BaseTime.AddSeconds(1) });
            Instruments.Add(new Instrument { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, Status = InstrumentStatus.Continous, EnterTime = BaseTime.AddSeconds(2) });
            Instruments.Add(new Instrument { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, Status = InstrumentStatus.Closed, EnterTime = BaseTime.AddHours(31) });
            Instruments.Add(new Instrument { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, Status = InstrumentStatus.Closed, EnterTime = BaseTime.AddHours(32) });

            /*
             * Ticks.
             */
            Ticks.Add(new Tick { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(11), LastPrice = 8900, Volume = 100, OpenInterest = 1000, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8900, AskVolume = 10, BidPrice = 8898, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(12), LastPrice = 8899, Volume = 110, OpenInterest = 1010, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8899, AskVolume = 10, BidPrice = 8898, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(13), LastPrice = 8897, Volume = 120, OpenInterest = 1020, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8898, AskVolume = 10, BidPrice = 8897, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(14), LastPrice = 8898, Volume = 130, OpenInterest = 1030, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8898, AskVolume = 10, BidPrice = 8896, BidVolume = 10 });

            Ticks.Add(new Tick { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(21), LastPrice = 8500, Volume = 100, OpenInterest = 1000, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8500, AskVolume = 10, BidPrice = 8498, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(22), LastPrice = 8499, Volume = 110, OpenInterest = 1010, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8499, AskVolume = 10, BidPrice = 8498, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(23), LastPrice = 8497, Volume = 120, OpenInterest = 1020, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8498, AskVolume = 10, BidPrice = 8497, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(24), LastPrice = 8498, Volume = 130, OpenInterest = 1030, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8498, AskVolume = 10, BidPrice = 8496, BidVolume = 10 });

            Configurator = new SimulatedConfigurator(Ticks, Instruments);
        }

        [TestMethod("Subcribe and receive feeds.")]
        public void ReceiveFeeds()
        {
            Configurator.Configure(out var _, out var feedSource);

            /*
             * Subscribe an instrument from feed source and handler receives only that instrument.
             * 
             * 1. Handler receives feeds of the subscribed instrument.
             * 2. Exchange listener receives connect status notice.
             */
            feedSource.Register(FeedHandler, FeedSourceExchange);
            feedSource.Subscribe("l2205");

            var simFeedSource = (SimulatedFeedSource)feedSource;

            /*
             * 1. Mock feeds, send the first instrument status update.
             */
            Assert.IsTrue(simFeedSource.Flip());
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
            Assert.IsTrue(simFeedSource.Flip());

            /*
             * Feed handler receives second instrument feed.
             */
            Assert.AreEqual(2, FeedHandler.Instruments.Count);
            Assert.AreEqual("pp2205", FeedHandler.Instruments[1].InstrumentID);
            Assert.AreEqual(InstrumentStatus.Continous, FeedHandler.Instruments[1].Status);

            /*
             * 2. Mock feeds, ticks are received.
             */
            for (int i = 0; i < Ticks.Count; ++i)
            {
                Assert.IsTrue(simFeedSource.Flip());
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
            Assert.IsTrue(simFeedSource.Flip());
            Assert.IsFalse(simFeedSource.Flip());

            /*
             * Check handler receives the last two instrument statuses.
             */
            Assert.AreEqual(4, FeedHandler.Instruments.Count);
            Assert.AreEqual("l2205", FeedHandler.Instruments[2].InstrumentID);
            Assert.AreEqual(InstrumentStatus.Closed, FeedHandler.Instruments[2].Status);
            Assert.AreEqual("pp2205", FeedHandler.Instruments[3].InstrumentID);
            Assert.AreEqual(InstrumentStatus.Closed, FeedHandler.Instruments[3].Status);
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
