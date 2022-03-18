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
    public class TickRecorderValidation
    {
        internal Tick Sample { get; private set; } = new Tick();

        [TestInitialize]
        public void Initialize()
        {
            Sample = new Tick
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
        }

        [TestMethod("Formatter and parser work.")]
        public void FormatterAndParser()
        {
            var line = TickRecorder.Format(Sample);
            
            Assert.IsTrue(TickRecorder.ParseTick(line, out var tick));
            Assert.AreEqual(Sample.InstrumentID, tick.InstrumentID);
            Assert.AreEqual(Sample.ExchangeID, tick.ExchangeID);
            Assert.AreEqual(Sample.Symbol, tick.Symbol);
            Assert.AreEqual(Sample.TradingDay, tick.TradingDay);
            Assert.AreEqual(Sample.TimeStamp, tick.TimeStamp);
            Assert.IsNull(tick.AveragePrice);
            Assert.AreEqual(Sample.LastPrice, tick.LastPrice);
            Assert.AreEqual(Sample.OpenPrice, tick.OpenPrice);
            Assert.AreEqual(Sample.HighPrice, tick.HighPrice);
            Assert.AreEqual(Sample.LowPrice, tick.LowPrice);
            Assert.IsNull(tick.ClosePrice);
            Assert.IsNull(tick.SettlementPrice);
            Assert.AreEqual(Sample.Volume, tick.Volume);
            Assert.AreEqual(Sample.OpenInterest, tick.OpenInterest);
            Assert.AreEqual(Sample.PreClosePrice, tick.PreClosePrice);
            Assert.AreEqual(Sample.PreSettlementPrice, tick.PreSettlementPrice);
            Assert.AreEqual(Sample.PreOpenInterest, tick.PreOpenInterest);
            Assert.AreEqual(Sample.AskPrice, tick.AskPrice);
            Assert.AreEqual(Sample.AskVolume, tick.AskVolume);
            Assert.AreEqual(Sample.BidPrice, tick.BidPrice);
            Assert.AreEqual(Sample.BidVolume, tick.BidVolume);
        }
    }
}
