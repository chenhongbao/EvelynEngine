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
    internal class MockedFeedSource : IFeedSource
    {
        private IFeedHandler? _feedHandler;
        private IExchangeListener? _exchangeListener;

        private IFeedHandler Handler => _feedHandler ?? throw new NullReferenceException("Feed handler has no value.");

        public void Subscribe(string instrumentID)
        {
            SubscribedInstruments.Add(instrumentID);
        }

        public void Unsubscribe(string instrumentID)
        {
            UnsubscribedInstruments.Add(instrumentID);
        }

        public void Unsubscribe(string instrumentID, IFeedHandler feedHandler)
        {
            UnsubscribedInstruments.Add(instrumentID);
        }

        public void Register(IFeedHandler feedHandler, IExchangeListener exchangeListener)
        {
            _feedHandler = feedHandler;
            _exchangeListener = exchangeListener;
        }

        #region Mocking Methods
        internal void MockedReceive(Tick tick)
        {
            Handler.OnFeed(tick);
        }

        internal void MockedReceive(OHLC ohlc)
        {
            Handler.OnFeed(ohlc);
        }

        internal void MockedReceive(Instrument instrument)
        {
            Handler.OnInstrument(instrument);

            System.Console.Error.WriteLine("SEND: " + instrument.InstrumentID);
        }

        internal void MockedReplySubscribe(string instrumentID, Description description, bool isSubscribed)
        {
            Handler.OnSubscribed(instrumentID, description, isSubscribed);
        }

        internal void MockedConnect(bool isConnected)
        {
            (_exchangeListener ?? throw new NullReferenceException("Exchange listener has no value.")).OnConnected(isConnected);
        }
        internal List<string> SubscribedInstruments { get; } = new List<string>();
        internal List<string> UnsubscribedInstruments { get; } = new List<string>();

        public DateOnly TradingDay => throw new NotImplementedException();
        #endregion
    }
}