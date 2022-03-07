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
using Evelyn.UnitTest.Behavior;
using Evelyn.UnitTest.Mock;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Evelyn.UnitTest.CLI
{
    [TestClass]
    public class EmptyEngine : DataInitialize
    {
        internal IEvelyn Engine { get; private set; } = IEvelyn.NewInstance;
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal MockedManagementService ManagementService { get; private set; } = new MockedManagementService();
        internal DateOnly TradingDay { get; } = DateOnly.FromDateTime(DateTime.Now);

        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();

            Configurator = new MockedConfigurator();

            Engine = IEvelyn.NewInstance;
            Engine.RegisterManagement(ManagementService)
                .Configure(Configurator);
        }


        [TestMethod("Get engine information with the most basic plugin reigstered.")]
        public void QueryEngineInformation()
        {
            /*
             * Engine is initialized with basic settings, and try getting engine information.
             * 
             * 1. Most information is null and exchange connect state is false.
             * 2. Connect the exchange and exchange connect state is true, and trading day is available.
             */
            var query = ManagementService.Management.QueryEngineInformation();

            var result = query.Result;
            var description = query.Description;

            /*
             * 1. Engine information is null or default.
             * 
             * Description is fine, no error.
             */
            Assert.AreEqual(0, description.Code);
            Assert.AreEqual(string.Empty, description.Message);

            /*
             * Engine information is null or default.
             */
            Assert.AreEqual(false, result.Broker.IsConnected);
            Assert.AreEqual(DateOnly.MaxValue, result.Broker.TradingDay);

            Assert.AreEqual(false, result.FeedSource.IsConnected);
            Assert.AreEqual(DateOnly.MaxValue, result.FeedSource.TradingDay);
            Assert.AreEqual(0, result.FeedSource.OHLCGeneratorCount);
            Assert.AreEqual(0, result.FeedSource.SubscriptionResponses.Count);
            Assert.AreEqual(0, result.FeedSource.ScheduledJobs.Count);

            /*
             * 2. Connect exchange and shall see the connect state changes and valid
             * trading day.
             */
            Configurator.Broker.MockedConnect(true);
            Configurator.FeedSource.MockedConnect(true);

            /*
             * Set valid trading day.
             */
            Configurator.Broker.TradingDay = TradingDay;
            Configurator.FeedSource.TradingDay = TradingDay;

            query = ManagementService.Management.QueryEngineInformation();

            result = query.Result;
            description = query.Description;

            /*
             * Query returns no error.
             */
            Assert.AreEqual(0, description.Code);
            Assert.AreEqual(string.Empty, description.Message);

            /*
             * Engine exchange is connected..
             */
            Assert.AreEqual(true, result.Broker.IsConnected);
            Assert.AreEqual(TradingDay, result.Broker.TradingDay);

            Assert.AreEqual(true, result.FeedSource.IsConnected);
            Assert.AreEqual(TradingDay, result.FeedSource.TradingDay);
            Assert.AreEqual(0, result.FeedSource.OHLCGeneratorCount);
            Assert.AreEqual(0, result.FeedSource.SubscriptionResponses.Count);
            Assert.AreEqual(0, result.FeedSource.ScheduledJobs.Count);
        }

        [TestMethod("Alter client returns error.")]
        public void AlterReturnError()
        {
            /*
             * Altering an non existing client returns error (21).
             */
            var alter = ManagementService.Management.AlterClient("ANY_CLIENT_ID", "l2205", "pp2205");

            Assert.AreEqual("ANY_CLIENT_ID", alter.Result.ClientID);
            Assert.AreEqual(21, alter.Description.Code);
            Assert.AreEqual("no such client", alter.Description.Message.Substring(0, 14).ToLower());
        }

        [TestMethod("Query client returns empty list, no error.")]
        public void QueryClientReturnEmpty()
        {
            /*
             * Query list of clients returns empty.
             */
            var query = ManagementService.Management.QueryClients();

            Assert.AreEqual(0, query.Result.Clients.Count);
            Assert.AreEqual(0, query.Description.Code);
        }

        [TestMethod("Query non existing client's order returns error.")]
        public void QueryClientOrderReturnEror()
        {
            /*
             * Query a non existing client's order returns error.
             */
            var query = ManagementService.Management.QueryClientOrder("ANY_CLIENT_ID", "ANY_ORDER_ID");

            Assert.AreEqual("ANY_CLIENT_ID", query.Result.ClientID);
            Assert.AreEqual(24, query.Description.Code);
            Assert.AreEqual("no such client", query.Description.Message.Substring(0, 14).ToLower());
        }

        [TestMethod("Query a non existing client's logs returns error.")]
        public void QueryClientLogReturnError()
        {
            /*
             * Query a non existing client's logs returns error.
             */
            var query = ManagementService.Management.QueryClientLog("ANY_CLIENT_ID", DateTime.Now);

            Assert.AreEqual("ANY_CLIENT_ID", query.Result.ClientID);
            Assert.AreEqual(LogLevel.None, query.Result.Level);
            Assert.AreEqual(0, query.Result.Logs.Count);
            Assert.AreEqual(DateTime.MaxValue, query.Result.LastLogTime);

            Assert.AreEqual(22, query.Description.Code);
            Assert.AreEqual("no such client", query.Description.Message.Substring(0, 14).ToLower());
        }
    }
}
