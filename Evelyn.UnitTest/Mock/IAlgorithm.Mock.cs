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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public void OnFeed(Instrument instrument)
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
            if (subscribed)
            {
                ReceivedSubscribe = (instrumentID, description);
            }
            else
            {
                ReceivedUnsubscribe = (instrumentID, description);
            }
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


        public string OnCommand(params string[] commands)
        {
            return commands.Aggregate((lhs, rhs) => String.Format("{0}_{1}", lhs, rhs));
        }

        #region Mocking Methods
        internal List<Tick> ReceivedTicks { get; } = new List<Tick>();
        internal List<OHLC> ReceivedOHLCs { get; } = new List<OHLC>();
        internal List<Instrument> ReceivedInstruments { get; } = new List<Instrument>();
        internal (string, Description) ReceivedSubscribe { get; set; } = (string.Empty, new Description());
        internal (string, Description) ReceivedUnsubscribe { get; set; } = (string.Empty, new Description());
        internal List<(Trade, Description)> ReceivedTrades { get; } = new List<(Trade, Description)>();
        internal bool IsLoaded { get; private set; } = false;
        internal bool IsUnloaded { get; private set; } = false;

        internal void MockedNewOrder(NewOrder newOrder, OrderOption? option = null)
        {
            (_op ?? throw new NullReferenceException("Operator has no value.")).New(newOrder, option);
        }

        internal void MockedDelete(DeleteOrder deleteOrder, OrderOption? option = null)
        {
            (_op ?? throw new NullReferenceException("Operator has no value.")).Delete(deleteOrder, option);
        }

        internal void MockLog(string format, params string[] args)
        {
            (_op ?? throw new NullReferenceException("Operator has no value.")).Logger.LogInformation(format, args);
        }
        #endregion
    }
}