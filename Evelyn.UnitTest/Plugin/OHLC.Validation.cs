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

namespace Evelyn.UnitTest.Plugin
{
    [TestClass]
    public class OHLCValidation
    {
        internal List<Tick> MockedTicks { get; private set; } = new List<Tick>();
        internal IEvelyn Engine { get; private set; } = IEvelyn.New();
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal MockedLocalClient Client { get; private set; } = new MockedLocalClient();
        internal DateOnly TradingDay { get; private set; }
        internal DateTime BaseTime { get; } = new DateTime(2022, 3, 2, 12, 44, 0);

        [TestInitialize]
        public void Initialize()
        {
            var baseTime = BaseTime;
            TradingDay = DateOnly.FromDateTime(baseTime);

            Engine = IEvelyn.New();

            MockedTicks.Clear();

            /*
             * Minute(1).
             */
            AddTick(baseTime.AddSeconds(1), 50, 1, 10);
            AddTick(baseTime.AddSeconds(15), 53, 4, 8);
            AddTick(baseTime.AddSeconds(25), 46, 6, 6);
            AddTick(baseTime.AddSeconds(55), 47, 8, 10);

            /*
             * Minute(2).
             */
            baseTime = baseTime.AddMinutes(1);

            AddTick(baseTime.AddSeconds(1), 48, 15, 10);
            AddTick(baseTime.AddSeconds(15), 49, 18, 11);
            AddTick(baseTime.AddSeconds(25), 56, 35, 15);
            AddTick(baseTime.AddSeconds(55), 55, 36, 15);

            /*
             * Minute(3).
             */
            baseTime = baseTime.AddMinutes(2);

            AddTick(baseTime.AddSeconds(1), 55, 37, 15);
            AddTick(baseTime.AddSeconds(15), 54, 40, 14);
            AddTick(baseTime.AddSeconds(25), 52, 44, 16);
            AddTick(baseTime.AddSeconds(55), 51, 45, 17);
        }

        private void AddTick(DateTime timeStamp, double lastPrice, int volume, int openInterest)
        {
            MockedTicks.Add(new Tick
            {
                InstrumentID = "l2205",
                TradingDay = TradingDay,
                TimeStamp = timeStamp,
                LastPrice = lastPrice,
                Volume = volume,
                OpenInterest = openInterest
            });
        }

        [TestMethod("Official OHLC generator works.")]
        public void OfficialOHLC()
        {
            Engine.RegisterLocalClient("MOCKED_CLIENT", Client, "l2205")
                .GenerateOHLC()
                .Configure(Configurator);

            /*
             * The first two minutes have OHLC, the last minute will not because
             * no more ticks, into next minute, to trigger the generation.
             */
            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));

            /*
             * Check client receives two OHLC.
             */
            Assert.AreEqual(2, Client.ReceivedOHLCs.Count);

            var ohlc0 = Client.ReceivedOHLCs[0];

            Assert.AreEqual("l2205", ohlc0.InstrumentID);
            Assert.AreEqual(TradingDay, ohlc0.TradingDay);
            Assert.AreEqual(BaseTime.AddSeconds(55), ohlc0.TimeStamp);
            Assert.AreEqual(50, ohlc0.OpenPrice);
            Assert.AreEqual(53, ohlc0.HighPrice);
            Assert.AreEqual(46, ohlc0.LowPrice);
            Assert.AreEqual(47, ohlc0.ClosePrice);
            Assert.AreEqual(10, ohlc0.OpenInterest);
            Assert.AreEqual(7, ohlc0.Volume);

            var ohlc1 = Client.ReceivedOHLCs[1];

            Assert.AreEqual("l2205", ohlc1.InstrumentID);
            Assert.AreEqual(TradingDay, ohlc1.TradingDay);
            Assert.AreEqual(BaseTime.AddMinutes(1).AddSeconds(55), ohlc1.TimeStamp);
            Assert.AreEqual(48, ohlc1.OpenPrice);
            Assert.AreEqual(56, ohlc1.HighPrice);
            Assert.AreEqual(48, ohlc1.LowPrice);
            Assert.AreEqual(55, ohlc1.ClosePrice);
            Assert.AreEqual(15, ohlc1.OpenInterest);
            Assert.AreEqual(28, ohlc1.Volume);
        }
    }
}
