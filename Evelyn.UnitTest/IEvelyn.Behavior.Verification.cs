﻿/*
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
using Evelyn.Plugin;
using Evelyn.UnitTest.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evelyn.UnitTest
{
    [TestClass]
    public class EvelynBehaviorVerfication
    {
        [TestMethod("IEvelyn calls IClientService's methods correctly.")]
        public void CallClientService()
        {
            IEvelyn engine = IEvelyn.New();

            var mockedClientService = new MockedClientService();
            var mockedConfiguator = new MockedConfigurator();

            engine.EnableRemoteClient(mockedClientService)
                .Configure(mockedConfiguator);

            // TODO Write test codes.
        }
    }
}
