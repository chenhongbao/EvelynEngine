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
using Evelyn.Plugin;
using System;

namespace Evelyn.UnitTest.Mock
{
    internal class MockedFeedSource : IFeedSource
    {
        public void Subscribe(string instrumentID, IFeedHandler feedHandler)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(string instrumentID)
        {
            throw new NotImplementedException();
        }

        #region Mocking Methods
        internal void MockedReceive(Tick tick)
        {
            throw new NotImplementedException();
        }

        internal void MockedReceive(OHLC ohlc)
        {
            throw new NotImplementedException();
        }

        internal void MockedReceive(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        internal void MockedReplySubscribe(string v1, Description description, bool isSubscribed, string clientID)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}