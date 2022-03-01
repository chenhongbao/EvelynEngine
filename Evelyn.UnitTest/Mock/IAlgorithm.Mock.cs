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
using System.Collections.Generic;

namespace Evelyn.UnitTest.Mock
{
    internal class MockedLocalClient : IAlgorithm
    {
        private IOperator? _op;
        public void OnFeed(Tick tick)
        {
            ReceivedTicks.Add(tick);
        }

        public void OnFeed(OHLC ohlc)
        {
            ReceivedOHLCs.Add(ohlc);
        }

        public void OnInstrument(Instrument instrument)
        {
            ReceivedInstruments.Add(instrument);
        }

        public void OnLoad(IOperator op)
        {
            _op = op;
            IsLoaded = true;
            IsUnloaded = false;
        }

        public void OnSubscribed(string instrumentID, Description description, bool subscribed)
        {
            ReceivedSubscribe = (instrumentID, description, subscribed);
        }

        public void OnTrade(Trade trade, Description description)
        {
            ReceivedTrades.Add((trade, description));
        }

        public void OnUnload()
        {
            IsUnloaded = true;
            IsLoaded = false;
        }

        #region Mocking Methods
        internal List<Tick> ReceivedTicks { get; } = new List<Tick>();
        internal List<OHLC> ReceivedOHLCs { get; } = new List<OHLC>();
        internal List<Instrument> ReceivedInstruments { get; } = new List<Instrument>();
        internal (string, Description, bool) ReceivedSubscribe { get; private set; } = (string.Empty, new Description(), false);
        internal List<(Trade, Description)> ReceivedTrades { get; } = new List<(Trade, Description)>();
        internal bool IsLoaded { get; private set; } = false;
        internal bool IsUnloaded { get; private set; } = false;

        internal void MockedNewOrder(NewOrder newOrder, OrderOption? option = null)
        {
            (_op ?? throw new NoValueException("Operator has no value.")).New(newOrder, option);
        }

        internal void MockedDelete(string orderID, OrderOption? option = null)
        {
            (_op ?? throw new NoValueException("Operator has no value.")).Delete(orderID, option);
        }
        #endregion
    }
}