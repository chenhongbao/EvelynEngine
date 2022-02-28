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
using System.Collections.Generic;

namespace Evelyn.UnitTest.Mock
{
    internal class MockedLocalClient : IAlgorithm
    {
        public void OnFeed(Tick tick)
        {
            throw new System.NotImplementedException();
        }

        public void OnFeed(OHLC ohlc)
        {
            throw new System.NotImplementedException();
        }

        public void OnInstrument(Instrument instrument)
        {
            throw new System.NotImplementedException();
        }

        public void OnLoad(IOperator op)
        {
            throw new System.NotImplementedException();
        }

        public void OnSubscribed(string instrumentID, Description description, bool subscribed)
        {
            throw new System.NotImplementedException();
        }

        public void OnTrade(Trade trade, Description description)
        {
            throw new System.NotImplementedException();
        }

        public void OnUnload()
        {
            throw new System.NotImplementedException();
        }

        #region Mocking Methods
        internal List<Tick> ReceivedTicks { get; } = new List<Tick>();
        internal List<OHLC> ReceivedOHLCs { get; } = new List<OHLC>();
        internal List<Instrument> ReceivedInstruments { get; } = new List<Instrument>();
        internal (string, Description, bool) ReceivedSubscribe { get; private set; }
        internal List<(Trade, Description)> ReceivedTrades { get; } = new List<(Trade, Description)>();
        internal bool IsLoaded { get; private set; } = false;
        internal bool IsUnloaded { get; private set; } = false;

        internal void MockedNewOrder(NewOrder newOrder)
        {
            throw new NotImplementedException();
        }

        internal void MockedDelete(string v)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}