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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evelyn.Extension.UnitTest
{
    [TestClass]
    public class FeedRecorderValidation
    {
        internal Tick SampleTick { get; private set; } = new Tick();
        internal OHLC SampleOHLC { get; private set; } = new OHLC();
        internal Instrument SampleInstrument { get; private set; } = new Instrument();

        [TestInitialize]
        public void Initialize()
        {
            SampleTick = new Tick
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                /* No symbol */
                TradingDay = System.DateOnly.FromDateTime(System.DateTime.Now),
                TimeStamp = System.DateTime.Now,
                /* No average price */
                LastPrice = 8990,
                OpenPrice = 8990,
                HighPrice = 8995,
                LowPrice = 8987,
                ClosePrice = null,
                SettlementPrice = null,
                Volume = 1909,
                OpenInterest = 89743,
                PreClosePrice = 8860,
                PreSettlementPrice = 8875,
                PreOpenInterest = 89054,
                AskPrice = 8991,
                AskVolume = 10,
                BidPrice = 8990,
                BidVolume = 10
            };

            SampleOHLC = new OHLC
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                /* No symbol */
                TradingDay = System.DateOnly.FromDateTime(System.DateTime.Now),
                TimeStamp = System.DateTime.Now,
                OpenPrice = 1,
                HighPrice = 3,
                LowPrice = 1,
                ClosePrice = 2,
                OpenInterest = 10,
                Volume = 5,
                Time = System.TimeSpan.FromMinutes(1)
            };

            SampleInstrument = new Instrument
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                Symbol = "塑料2205",
                TradingDay = System.DateOnly.FromDateTime(System.DateTime.Now),
                Status = InstrumentStatus.Continous,
                EnterTime = System.DateTime.Now
            };
        }

        [TestMethod("Tick formatter and parser.")]
        public void TickFormatterParser()
        {
            var line = FeedRecorder.Format(SampleTick);

            Assert.IsTrue(FeedRecorder.Parse(line, out Tick tick));
            Assert.AreEqual(SampleTick.InstrumentID, tick.InstrumentID);
            Assert.AreEqual(SampleTick.ExchangeID, tick.ExchangeID);
            Assert.AreEqual(SampleTick.Symbol, tick.Symbol);
            Assert.AreEqual(SampleTick.TradingDay, tick.TradingDay);
            Assert.AreEqual(SampleTick.TimeStamp, tick.TimeStamp);
            Assert.IsNull(tick.AveragePrice);
            Assert.AreEqual(SampleTick.LastPrice, tick.LastPrice);
            Assert.AreEqual(SampleTick.OpenPrice, tick.OpenPrice);
            Assert.AreEqual(SampleTick.HighPrice, tick.HighPrice);
            Assert.AreEqual(SampleTick.LowPrice, tick.LowPrice);
            Assert.IsNull(tick.ClosePrice);
            Assert.IsNull(tick.SettlementPrice);
            Assert.AreEqual(SampleTick.Volume, tick.Volume);
            Assert.AreEqual(SampleTick.OpenInterest, tick.OpenInterest);
            Assert.AreEqual(SampleTick.PreClosePrice, tick.PreClosePrice);
            Assert.AreEqual(SampleTick.PreSettlementPrice, tick.PreSettlementPrice);
            Assert.AreEqual(SampleTick.PreOpenInterest, tick.PreOpenInterest);
            Assert.AreEqual(SampleTick.AskPrice, tick.AskPrice);
            Assert.AreEqual(SampleTick.AskVolume, tick.AskVolume);
            Assert.AreEqual(SampleTick.BidPrice, tick.BidPrice);
            Assert.AreEqual(SampleTick.BidVolume, tick.BidVolume);
        }

        [TestMethod("OHLC formatter and parser.")]
        public void OHLCFormatterParser()
        {
            var line = FeedRecorder.Format(SampleOHLC);

            Assert.IsTrue(FeedRecorder.Parse(line, out OHLC ohlc));
            Assert.AreEqual(SampleOHLC, ohlc);
        }

        [TestMethod("Instrument formatter and parser.")]
        public void InstrumentFormatterParser()
        {
            var line = FeedRecorder.Format(SampleInstrument);

            Assert.IsTrue(FeedRecorder.Parse(line, out Instrument instrument));
            Assert.AreEqual(SampleInstrument, instrument);
        }
    }
}
