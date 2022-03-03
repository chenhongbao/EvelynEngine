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
#define EVELYN_LOGGER

using Evelyn.Internal.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;

namespace Evelyn.UnitTest.Logging
{
    [TestClass]
    public class EvelynLoggerVerification
    {
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
            Loggers.Writer = new StringWriter();

            var logger = new EvelynLogger(nameof(EvelynLoggerVerification), Loggers.Writer);

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
            System.Console.Error.WriteLine(Loggers.Writer.ToString());
        }
    }
}
