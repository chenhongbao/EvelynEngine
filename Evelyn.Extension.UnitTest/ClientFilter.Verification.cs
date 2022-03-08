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
using Evelyn.Extension.Client;
using Evelyn.Model;
using Evelyn.UnitTest.Behavior;
using Evelyn.UnitTest.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Evelyn.Extension.UnitTest 
{
    [TestClass]
    public class ClientFilterVerification : DataInitialize
    {
        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();
        }

        [TestMethod("Filter feeds.")]
        public void FilterFeed()
        {
            var mocked = new MockedLocalClient();
            var client = new AlgorithmClient("UNIT_TEST_CLIENT", mocked)
                .Subscribe("l2205", "pp2205")
                .AddFilter(new TestFilter());

            /*
             * Algorithm client filters feeds with given filter.
             * 
             * 1. Let filter block all ticks and ohlc, client receives no data.
             * 2. Let filter bypass all ticks and ohlc, client receives data.
             * 3. Let filter again block all ticks and ohlc, client afgain receives no data.
             */

            client.OnFeed(new Tick());
            client.OnFeed(new OHLC());
            client.OnFeed(new Instrument { Status = InstrumentStatus.AuctionOrdering});


            /*
             * 1. Client receives no data.
             */
            Assert.AreEqual(0, mocked.ReceivedTicks.Count);
            Assert.AreEqual(0, mocked.ReceivedOHLCs.Count);
            Assert.AreEqual(0, mocked.ReceivedInstruments.Count);

            /*
             * Input an instrument with wanted status, client receives the instrument.
             */
            client.OnFeed(new Instrument { Status = InstrumentStatus.NoTrading });

            Assert.AreEqual(1, mocked.ReceivedInstruments.Count);
            Assert.AreEqual(InstrumentStatus.NoTrading, mocked.ReceivedInstruments.Last().Status);

            /*
             * 2. Unblock the feeds by passing a Conitnous instrument status.
             */
            client.OnFeed(new Instrument { Status = InstrumentStatus.Continous });

            Assert.AreEqual(2, mocked.ReceivedInstruments.Count);
            Assert.AreEqual(InstrumentStatus.Continous, mocked.ReceivedInstruments.Last().Status);

            client.OnFeed(new Tick());
            client.OnFeed(new OHLC());

            Assert.AreEqual(1, mocked.ReceivedTicks.Count);
            Assert.AreEqual(1, mocked.ReceivedOHLCs.Count);

            /*
             * But if the instrument status is still not wanted, instrument is not received.
             */
            client.OnFeed(new Instrument { Status = InstrumentStatus.AuctionBalance });

            Assert.AreEqual(2, mocked.ReceivedInstruments.Count);

            /*
             * 3. Now block feeds again.
             * 
             * New status is set to ActionBalance, no need to send more.
             */
            client.OnFeed(new Tick());
            client.OnFeed(new OHLC());

            Assert.AreEqual(1, mocked.ReceivedTicks.Count);
            Assert.AreEqual(1, mocked.ReceivedOHLCs.Count);
        }
    }

    internal class TestFilter : Filter
    {
        private InstrumentStatus _status = InstrumentStatus.NoTrading;

        public override bool DoFeed(Tick tick) => _status == InstrumentStatus.Continous;

        public override bool DoFeed(OHLC ohlc) => _status == InstrumentStatus.Continous;

        public override bool DoFeed(Instrument instrument)
        {
            _status = instrument.Status;
            return instrument.Status == InstrumentStatus.NoTrading || instrument.Status == InstrumentStatus.Continous;
        }
    }
}
