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
    public class FeedSourceBehaviorVerification : EvelynBehaviorVerfication
    {
        internal IEvelyn Engine { get; private set; } = IEvelyn.New();
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal MockedLocalClient ClientA { get; private set; } = new MockedLocalClient();
        internal MockedLocalClient ClientB { get; private set; } = new MockedLocalClient();
        internal DateOnly TradingDay { get; private set; } = DateOnly.MaxValue;

        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();

            var baseTime = DateTime.Now;

            Engine = IEvelyn.New();
            TradingDay = DateOnly.FromDateTime(baseTime);

            Engine.RegisterInstrument(
                 new Instrument
                 {
                     InstrumentID = "l2205",
                     TradingDay = TradingDay,
                     TimeStamp = baseTime,
                     Margin = 0.11,
                     Commission = 1.01,
                     Multiple = 5,
                     MarginMethod = CalculationMethod.PerAmount,
                     CommissionMethod = CalculationMethod.PerVolume,
                     State = InstrumentState.Continous,
                     StateTimestamp = baseTime
                 },
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = TradingDay,
                    TimeStamp = baseTime,
                    Margin = 0.11,
                    Commission = 1.01,
                    Multiple = 5,
                    MarginMethod = CalculationMethod.PerAmount,
                    CommissionMethod = CalculationMethod.PerVolume,
                    State = InstrumentState.Closed,
                    StateTimestamp = baseTime
                })
                .Configure(Configurator);
        }

        [TestMethod("Subscribe for an instrument many times.")]
        public void SubscribeManyTimes()
        {
            /*
             * There are many clients subscribing for the same instrument and feed source
             * only receives the request at the first subscription.
             * 
             * 1. Client A subscribes for the instrument.
             * 2. Feed source receives the subscription request.
             * 3. Feed source sends market data and client A receives data.
             * 4. Client B subscribes for the same instrument.
             * 5. Feed source doesn't receive a duplicated subscription request.
             * 6. Feed source sends the market data and both clients receive data.
             */

            /*
             * 1. Client A subscribes for instrument l2205.
             */
            Engine.RegisterLocalClient("MOCKED_CLIENT_A", ClientA, "l2205");

            /*
             * 2. Feed source receives the request.
             */
            Assert.AreEqual(1, Configurator.FeedSource.SubscribedInstruments.Count);
            Assert.AreEqual("l2205", Configurator.FeedSource.SubscribedInstruments[0]);

            /*
             * Feed source sends subscription response and client A receives the response.
             */
            Configurator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK-l2205" }, true);

            Assert.AreEqual("l2205", ClientA.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, ClientA.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK-l2205", ClientA.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(true, ClientA.ReceivedSubscribe.Item3);

            /*
             * 3. Feed source sends market data and client A receives the data.
             */
            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => Configurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => Configurator.FeedSource.MockedReceive(instrument));

            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), ClientA.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), ClientA.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), ClientA.ReceivedInstruments);

            /*
             * 4. Client B subscribes for the same instrument.
             */
            Engine.RegisterLocalClient("MOCKED_CLIENT_B", ClientB, "l2205");

            /*
             * 5. Feed source doesn't receive the request, but engine will send a subscription response to client B.
             */
            Assert.AreEqual(1, Configurator.FeedSource.SubscribedInstruments.Count);
            Assert.AreEqual("l2205", Configurator.FeedSource.SubscribedInstruments[0]);

            /*
             * Check engine sends a subscription response to client B.
             */
            Assert.AreEqual("l2205", ClientB.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, ClientB.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK-l2205", ClientB.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(true, ClientB.ReceivedSubscribe.Item3);

            /*
             * 6. Feed source sends market data and both clients receive the data.
             */
            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => Configurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => Configurator.FeedSource.MockedReceive(instrument));

            /*
             * Client A.
             */
            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), ClientA.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), ClientA.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), ClientA.ReceivedInstruments);

            /*
             * Client B.
             */
            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), ClientB.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), ClientB.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), ClientB.ReceivedInstruments);
        }

        [TestMethod("Unsubscribe for an instrument many times.")]
        public void UnsubscribeManyTimes()
        {
            /*
             * There are many clients subscribing for the same instrument, and then
             * unsubscribe it.
             * 
             * Feed source only receives the unsubscription request at the last
             * unsubscription.
             * 
             * 1. Client A and B both subscribe for an instrument.
             * 2. Feed source sends market data and both A and B receive the data.
             * 3. Client A unsubscribes the instrument.
             * 4. Feed source doesn't receive an unsubsciption request.
             * 5. Client A receives no data, while client B still receives data.
             * 6. Client B unsubscribes the instrument.
             * 7. Feed source receives unsubscription request.
             * 8. Feed source sends market data, and none of the clients receives the data.
             */

            /*
             * 1. Client A and client B subscribe for the same instrument.
             */
            Engine.RegisterLocalClient("MOCKED_CLIENT_A", ClientA, "l2205");
            Engine.RegisterLocalClient("MOCKED_CLIENT_B", ClientB, "l2205");

            /*
             * Feed source only receives 1 subscription request and sends the response.
             */
            Assert.AreEqual(1, Configurator.FeedSource.SubscribedInstruments.Count);
            Assert.AreEqual("l2205", Configurator.FeedSource.SubscribedInstruments[0]);

            Configurator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK-l2205" }, true);

            /*
             * Both client A and client B receive subscription response.
             */
            Assert.AreEqual("l2205", ClientA.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, ClientA.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK-l2205", ClientA.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(true, ClientA.ReceivedSubscribe.Item3);

            Assert.AreEqual("l2205", ClientB.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, ClientB.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK-l2205", ClientB.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(true, ClientB.ReceivedSubscribe.Item3);

            /*
             * 2. Feed source sends the market data and both clients receive the data.
             */
            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => Configurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => Configurator.FeedSource.MockedReceive(instrument));

            /*
             * Client A.
             */
            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), ClientA.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), ClientA.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), ClientA.ReceivedInstruments);

            /*
             * Client B.
             */
            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), ClientB.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), ClientB.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), ClientB.ReceivedInstruments);

            /*
             * 3. Client A unsusbcribes the instrument.
             */
            Engine.AlterLocalClient("MOCKED_CLIENT_A", "");

            /*
             * 4. Feed source doesn't receive the unsubscription request and engine sends an unsubscription 
             * response to client A.
             */
            Assert.AreEqual(0, Configurator.FeedSource.UnsubscribedInstruments.Count);

            /*
             * Client A receives an unsubscription response.
             */
            Assert.AreEqual("l2205", ClientA.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, ClientA.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK", ClientA.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(false, ClientA.ReceivedSubscribe.Item3);

            /*
             * Client B doesn't receive the unsubscription response.
             */
            Assert.AreEqual(string.Empty, ClientB.ReceivedSubscribe.Item1);

            /*
             * 5. Client A no more receives data, while client B still receives data.
             */
            ClientA.ReceivedTicks.Clear();
            ClientA.ReceivedOHLCs.Clear();
            ClientA.ReceivedInstruments.Clear();

            ClientB.ReceivedTicks.Clear();
            ClientB.ReceivedOHLCs.Clear();
            ClientB.ReceivedInstruments.Clear();

            /*
             * Feed source sends the market data.
             */
            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => Configurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => Configurator.FeedSource.MockedReceive(instrument));

            /*
             * Client A.
             */
            Assert.AreEqual(0, ClientA.ReceivedTicks.Count);
            Assert.AreEqual(0, ClientA.ReceivedOHLCs.Count);
            Assert.AreEqual(0, ClientA.ReceivedInstruments.Count);

            /*
             * Client B.
             */
            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), ClientB.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), ClientB.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), ClientB.ReceivedInstruments);

            /*
             * 6. Client B unsubscribes the instrument.
             */
            Engine.AlterLocalClient("MOCKED_CLIENT_B", "");

            /*
             * 7. Feed source receives the unsubscription request and sends an unsubscription response.
             */
            Assert.AreEqual(1, Configurator.FeedSource.UnsubscribedInstruments.Count);
            Assert.AreEqual("l2205", Configurator.FeedSource.UnsubscribedInstruments[0]);

            Configurator.FeedSource.MockedReplySubscribe("l2205", new Description { Code = 0, Message = "OK" }, false);

            /*
             * Client B receives the unsubscription response.
             */
            Assert.AreEqual("l2205", ClientB.ReceivedSubscribe.Item1);
            Assert.AreEqual(0, ClientB.ReceivedSubscribe.Item2.Code);
            Assert.AreEqual("OK", ClientB.ReceivedSubscribe.Item2.Message);
            Assert.AreEqual(false, ClientB.ReceivedSubscribe.Item3);

            /*
             * Client A doesn't receive the response.
             */
            Assert.AreEqual(string.Empty, ClientA.ReceivedSubscribe.Item1);

            /*
             * 8. Feed source sends the market data and none of the clients receive the data.
             */
            ClientA.ReceivedTicks.Clear();
            ClientA.ReceivedOHLCs.Clear();
            ClientA.ReceivedInstruments.Clear();

            ClientB.ReceivedTicks.Clear();
            ClientB.ReceivedOHLCs.Clear();
            ClientB.ReceivedInstruments.Clear();

            /*
             * Feed source sends the market data.
             */
            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => Configurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => Configurator.FeedSource.MockedReceive(instrument));

            /*
             * Client A.
             */
            Assert.AreEqual(0, ClientA.ReceivedTicks.Count);
            Assert.AreEqual(0, ClientA.ReceivedOHLCs.Count);
            Assert.AreEqual(0, ClientA.ReceivedInstruments.Count);

            /*
             * Client B.
             */
            Assert.AreEqual(0, ClientB.ReceivedTicks.Count);
            Assert.AreEqual(0, ClientB.ReceivedOHLCs.Count);
            Assert.AreEqual(0, ClientB.ReceivedInstruments.Count);
        }
    }
}
