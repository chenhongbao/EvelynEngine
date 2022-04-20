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
using Evelyn.Internal;
using Evelyn.Model;
using Evelyn.Model.CLI;
using Evelyn.Plugin;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Evelyn.Extension.UnitTest
{
    [TestClass]
    public class CLIVerification
    {
        private MockedManagement Service { get; set; } = new MockedManagement();
        private ManagementConsole MngConsole { get; init; } = new ManagementConsole();

        [TestInitialize]
        public void Initialize()
        {
            Service = new MockedManagement();
        }

        [TestMethod("Check command syntax.")]
        public void CheckCommandSyntax()
        {
            /*
             * Common case.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("SendCommand CLIENT_ID \"COMMAND MESSAGE\"")?.Invoke(Service));
            Assert.AreEqual("CLIENT_ID", Service.Command.Item1);
            Assert.AreEqual(1, Service.Command.Item2.Length);
            Assert.AreEqual("COMMAND MESSAGE", Service.Command.Item2[0]);

            /*
             * Blank as prefix.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("\u0020\u0020SendCommand CLIENT_ID \"COMMAND MESSAGE\"")?.Invoke(Service));
            Assert.AreEqual("CLIENT_ID", Service.Command.Item1);
            Assert.AreEqual(1, Service.Command.Item2.Length);
            Assert.AreEqual("COMMAND MESSAGE", Service.Command.Item2[0]);

            /*
             * All string parameter has quotes.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("\u0020\u0020SendCommand \"CLIENT ID\" \"2022-03-25 16:44\"")?.Invoke(Service));
            Assert.AreEqual("CLIENT ID", Service.Command.Item1);
            Assert.AreEqual(1, Service.Command.Item2.Length);
            Assert.AreEqual("2022-03-25 16:44", Service.Command.Item2[0]);
        }

        [TestMethod("Alter client.")]
        public void CallAlterClient()
        {
            /*
             * Common case.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("AlterClient CLIENT_ID l2205 pp2205")?.Invoke(Service));
            Assert.AreEqual("CLIENT_ID", Service.Alter.Item1);
            Assert.AreEqual(2, Service.Alter.Item2.Length);

            /*
             * No instruments.
             * Error.
             */
            var result = MngConsole.ParseCommand("AlterClient CLIENT_ID")?.Invoke(Service);
            Assert.IsNotNull(result);
            Assert.AreNotEqual(ErrorCodes.OK, result?.Description.Code);
        }

        [TestMethod("Query client logs.")]
        public void CallQueryClientLog()
        {
            /*
             * Common case.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("QueryClientLog CLIENT_ID \"2022-03-25 17:22:22\" Debug")?.Invoke(Service));
            Assert.AreEqual("CLIENT_ID", Service.QueryLogs.Item1);
            Assert.AreEqual(new DateTime(2022, 3, 25, 17, 22, 22), Service.QueryLogs.Item2);
            Assert.AreEqual(LogLevel.Debug, Service.QueryLogs.Item3);

            /*
             * Query with default parameter.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("QueryClientLog CLIENT_ID \"2022-03-25 17:22:22\"")?.Invoke(Service));
            Assert.AreEqual("CLIENT_ID", Service.QueryLogs.Item1);
            Assert.AreEqual(new DateTime(2022, 3, 25, 17, 22, 22), Service.QueryLogs.Item2);
            Assert.AreEqual(LogLevel.None, Service.QueryLogs.Item3);

            /*
             * Query with short datetime.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("QueryClientLog CLIENT_ID \"2022-03-25\"")?.Invoke(Service));
            Assert.AreEqual("CLIENT_ID", Service.QueryLogs.Item1);
            Assert.AreEqual(new DateTime(2022, 3, 25), Service.QueryLogs.Item2);
            Assert.AreEqual(LogLevel.None, Service.QueryLogs.Item3);
        }

        [TestMethod("Query client order.")]
        public void CallQueryClientOrder()
        {
            /*
             * Common case.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("QueryClientOrder CLIENT_ID ORDER_ID")?.Invoke(Service));
            Assert.AreEqual("CLIENT_ID", Service.ClientOrder.Item1);
            Assert.AreEqual("ORDER_ID", Service.ClientOrder.Item2);
        }

        [TestMethod("Query engine.")]
        public void CallQueryEngine()
        {
            /*
             * Common case.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("QueryEngineInformation")?.Invoke(Service));
            Assert.IsTrue(Service.Engine);

            /*
             * Query with unused parameters.
             */
            var result = MngConsole.ParseCommand("QueryEngineInformation UN_USED_PARAMETER")?.Invoke(Service);
            Assert.IsNotNull(result);
            Assert.AreNotEqual(ErrorCodes.OK, result?.Description.Code);
        }

        [TestMethod("Query clients.")]
        public void CallQueryClients()
        {
            /*
             * Common case.
             */
            Assert.IsNotNull(MngConsole.ParseCommand("QueryClients")?.Invoke(Service));
            Assert.IsTrue(Service.Clients);

            /*
             * Query with unused parameters.
             */
            var result = MngConsole.ParseCommand("QueryClients UN_USED_PARAMETER")?.Invoke(Service);
            Assert.IsNotNull(result);
            Assert.AreNotEqual(ErrorCodes.OK, result?.Description.Code);
        }

        [TestMethod("Exit system.")]
        public void CallExitSystem()
        {
            Assert.IsNotNull(MngConsole.ParseCommand("ExitSystem")?.Invoke(Service));

            /*
             * Check method called.
             */
            Assert.IsTrue(Service.Exit);
        }
    }

    internal class MockedManagement : IManagement
    {
        internal (string, string[]) Command { get; set; }
        internal (string, string[]) Alter { get; set; }
        internal (string, DateTime, LogLevel) QueryLogs { get; set; }
        internal (string, string) ClientOrder { get; set; }
        internal bool Engine { get; set; } = false;
        internal bool Clients { get; set; } = false;
        internal bool Exit { get; set; } = false;

        public ManagementResult<AlterClientResult> AlterClient(string clientID, params string[] instrumentID)
        {
            Alter = (clientID, instrumentID);
            return new ManagementResult<AlterClientResult>();
        }

        public ManagementResult<bool> ExitSystem()
        {
            Exit = true;
            return new ManagementResult<bool> { Result = true };
        }

        public ManagementResult<ClientLogInformation> QueryClientLog(string clientID, DateTime afterTime, LogLevel logLevel = LogLevel.None)
        {
            QueryLogs = (clientID, afterTime, logLevel);
            return new ManagementResult<ClientLogInformation>();
        }

        public ManagementResult<ClientOrderInformation> QueryClientOrder(string clientID, string orderID)
        {
            ClientOrder = (clientID, orderID);
            return new ManagementResult<ClientOrderInformation>();
        }

        public ManagementResult<ClientsInformation> QueryClients()
        {
            Clients = true;
            return new ManagementResult<ClientsInformation>();
        }

        public ManagementResult<EngineInformation> QueryEngineInformation()
        {
            Engine = true;
            return new ManagementResult<EngineInformation>();
        }

        public ManagementResult<string> SendCommand(string clientID, params string[] commands)
        {
            Command = (clientID, commands);
            return new ManagementResult<string>();
        }
    }
}
