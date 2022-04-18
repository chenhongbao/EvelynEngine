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
using Evelyn.Extension.CLI;
using Evelyn.Model;
using Evelyn.UnitTest.Behavior;
using Evelyn.UnitTest.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Evelyn.Extension.UnitTest
{
    [TestClass]
    public class Management : DataInitialize
    {
        internal IEvelyn Engine { get; private set; } = IEvelyn.NewInstance;
        internal MockedLocalClient ClientA { get; private set; } = new MockedLocalClient();
        internal MockedLocalClient ClientB { get; private set; } = new MockedLocalClient();
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal MockedClientService ClientService { get; private set; } = new MockedClientService();
        internal MockedManagementService ManagementService { get; private set; } = new MockedManagementService();

        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();

            Engine = IEvelyn.NewInstance;

            ClientA = new MockedLocalClient();
            ClientB = new MockedLocalClient();
            Configurator = new MockedConfigurator();

            /*
             * Engine is fully configured.
             */
            Engine.RegisterRemoteClient(ClientService)
                .RegisterLocalClient("MOCKED_CLIENT_A", ClientA, "l2205", "pp2205")
                .RegisterLocalClient("MOCKED_CLIENT_B", ClientB, "l2205")
                .RegisterManagement(ManagementService)
                .Configure(Configurator);

            /*
             * Exchange is connected.
             */
            Configurator.Broker.MockedConnect(true);
            Configurator.FeedSource.MockedConnect(true);
        }

        [TestMethod("Pause and resume client.")]
        public void CallPauseClient()
        {
            /*
             * Pause a client so it will not receive any feeds, and then resume it.
             * 
             * 1. Send feeds and check clients receive them.
             * 2. Pause one of the clients and check the paused client receives no more feeds, while
             *    the other working client receives feeds.
             * 3. Resume the paused client and send feeds again, the resumed client receives feeds.
             */

            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => Configurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => Configurator.FeedSource.MockedReceive(instrument));

            /*
             * 1. Check both clients receive their feeds.
             */
            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205" || tick.InstrumentID == "pp2205").ToList(), ClientA.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205" || ohlc.InstrumentID == "pp2205").ToList(), ClientA.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205" || instrument.InstrumentID == "pp2205").ToList(), ClientA.ReceivedInstruments);

            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), ClientB.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), ClientB.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), ClientB.ReceivedInstruments);

            /*
             * 2. Pause client A and it receives no more feeds.
             */
            var r = ManagementService.Management.PauseClient("MOCKED_CLIENT_A");

            Assert.IsTrue(r.Result);

            ClientA.ReceivedTicks.Clear();
            ClientA.ReceivedOHLCs.Clear();
            ClientA.ReceivedInstruments.Clear();

            ClientB.ReceivedTicks.Clear();
            ClientB.ReceivedOHLCs.Clear();
            ClientB.ReceivedInstruments.Clear();

            /*
             * Send feeds again, client A receives no feeds, and client B receives feeds.
             */
            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => Configurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => Configurator.FeedSource.MockedReceive(instrument));

            Assert.AreEqual(0, ClientA.ReceivedTicks.Count);
            Assert.AreEqual(0, ClientA.ReceivedOHLCs.Count);
            Assert.AreEqual(0, ClientA.ReceivedInstruments.Count);

            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), ClientB.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), ClientB.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), ClientB.ReceivedInstruments);

            /*
             * 3. Resume client A, and it receives feeds again.
             */
            r = ManagementService.Management.ResumeClient("MOCKED_CLIENT_A");

            Assert.IsTrue(r.Result);

            ClientA.ReceivedTicks.Clear();
            ClientA.ReceivedOHLCs.Clear();
            ClientA.ReceivedInstruments.Clear();

            ClientB.ReceivedTicks.Clear();
            ClientB.ReceivedOHLCs.Clear();
            ClientB.ReceivedInstruments.Clear();

            /*
             * Send feeds.
             */
            MockedTicks.ForEach(tick => Configurator.FeedSource.MockedReceive(tick));
            MockedOHLCs.ForEach(ohlc => Configurator.FeedSource.MockedReceive(ohlc));
            MockedInstruments.ForEach(instrument => Configurator.FeedSource.MockedReceive(instrument));

            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205" || tick.InstrumentID == "pp2205").ToList(), ClientA.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205" || ohlc.InstrumentID == "pp2205").ToList(), ClientA.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205" || instrument.InstrumentID == "pp2205").ToList(), ClientA.ReceivedInstruments);

            CompareCollection(MockedTicks.Where(tick => tick.InstrumentID == "l2205").ToList(), ClientB.ReceivedTicks);
            CompareCollection(MockedOHLCs.Where(ohlc => ohlc.InstrumentID == "l2205").ToList(), ClientB.ReceivedOHLCs);
            CompareCollection(MockedInstruments.Where(instrument => instrument.InstrumentID == "l2205").ToList(), ClientB.ReceivedInstruments);
        }
    }
}
