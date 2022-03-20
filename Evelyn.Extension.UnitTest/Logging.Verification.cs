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
using Evelyn.Extension.Logging;
using Evelyn.UnitTest.Mock;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;

namespace Evelyn.Extension.UnitTest
{
    [TestClass]
    public class LoggingVerification
    {
        internal IEvelyn Engine { get; private set; } = IEvelyn.NewInstance;
        internal MockedLocalClient ClientA { get; private set; } = new MockedLocalClient();
        internal MockedLocalClient ClientB { get; private set; } = new MockedLocalClient();
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal MockedClientService ClientService { get; private set; } = new MockedClientService();
        internal MockedManagementService ManagementService { get; private set; } = new MockedManagementService();

        [TestInitialize]
        public void Initialize()
        {
            Engine = IEvelyn.NewInstance;

            ClientA = new MockedLocalClient();
            ClientB = new MockedLocalClient();
            Configurator = new MockedConfigurator();

            /*
             * Engine is fully configured.
             */
            Engine.RegisterLocalClient("MOCKED_CLIENT_A", ClientA)
                .RegisterLocalClient("MOCKED_CLIENT_B", ClientB)
                .RegisterManagement(ManagementService)
                .Configure(Configurator);
        }

        [TestMethod("Log and query logs.")]
        public void LogAndQueryLogs()
        {
            ClientA.MockLog("Client A is writing information - 1.");
            ClientA.MockLog("Client A is writing information - 2.");

            var logsA = ManagementService.Management.QueryClientLog("MOCKED_CLIENT_A", DateTime.MinValue);

            /*
             * Check there are two logs.
             */
            Assert.AreEqual("MOCKED_CLIENT_A", logsA.Result.ClientID);
            Assert.AreEqual(2, logsA.Result.Logs.Count);

            ClientB.MockLog("Client B is writing information - 1.");

            var logsB = ManagementService.Management.QueryClientLog("MOCKED_CLIENT_B", DateTime.MinValue);

            /*
             * Check there are two logs.
             */
            Assert.AreEqual("MOCKED_CLIENT_B", logsB.Result.ClientID);
            Assert.AreEqual(1, logsB.Result.Logs.Count);
        }

        [TestMethod("Check scopes has correct indentation.")]
        public void ScopesIndentation()
        {
            /*
             * Write logs to a string builder so the content can be checked.
             * 
             * 1. Log in default scope.
             * 2. Log in outter scope.
             * 3. Log in inner scope.
             */
            EvelynLoggerProvider.Writer = new StringWriter();

            var logger = new EvelynLogger(nameof(LoggingVerification), EvelynLoggerProvider.Writer);

            logger.LogInformation("It is logging information 1.");

            using (logger.BeginScope("OutterScope"))
            {
                logger.LogInformation("It is logging outter scope 1.");

                using (logger.BeginScope("InnerScope"))
                {
                    logger.LogInformation("It is logging inner scope 1.\nIt is logging next line.");
                    logger.LogCritical("{0}\n{1}", "It is logging exception message.", new StackTrace().ToString());
                }

                logger.LogInformation("It is logging outter scope 2.");
            }

            logger.LogDebug("It is logging debug 1.");
            logger.LogWarning("{0}\n{1}", "It is logging exception message.", new StackTrace().ToString());

            /*
             * Check each scope has an extra indentation, and default scope has no indentation.
             */
            System.Console.Error.WriteLine(EvelynLoggerProvider.Writer.ToString());
        }
    }
}
