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
        private IDictionary<string, ISet<IFeedHandler>> _feeds = new Dictionary<string, ISet<IFeedHandler>>();

        public void Subscribe(string instrumentID, IFeedHandler feedHandler)
        {
            SubscribedInstruments.Add(instrumentID);

            if (!_feeds.ContainsKey(instrumentID))
            {
                _feeds.Add(instrumentID, new HashSet<IFeedHandler>());
            }
            _feeds[instrumentID].Add(feedHandler);
        }

        public void Unsubscribe(string instrumentID)
        {
            UnsubscribedInstruments.Add(instrumentID);
        }

        public void Unsubscribe(string instrumentID, IFeedHandler feedHandler)
        {
            UnsubscribedInstruments.Add(instrumentID);
        }

        #region Mocking Methods
        internal void MockedReceive(Tick tick)
        {
            if (_feeds.ContainsKey(tick.InstrumentID))
            {
                foreach (var handler in _feeds[tick.InstrumentID])
                {
                    handler.OnFeed(tick);
                }
            }
        }

        internal void MockedReceive(OHLC ohlc)
        {
            if (_feeds.ContainsKey(ohlc.InstrumentID))
            {
                foreach (var handler in _feeds[ohlc.InstrumentID])
                {
                    handler.OnFeed(ohlc);
                }
            }
        }

        internal void MockedReceive(Instrument instrument)
        {
            if (_feeds.ContainsKey(instrument.InstrumentID))
            {
                foreach (var handler in _feeds[instrument.InstrumentID])
                {
                    handler.OnInstrument(instrument);
                }
            }
        }

        internal void MockedReplySubscribe(string instrumentID, Description description, bool isSubscribed)
        {
            if (_feeds.ContainsKey(instrumentID))
            {
                foreach (var handler in _feeds[instrumentID])
                {
                    handler.OnSubscribed(instrumentID, description, isSubscribed);
                }
            }
        }

        internal List<string> SubscribedInstruments { get; } = new List<string>();
        internal List<string> UnsubscribedInstruments { get; } = new List<string>();
        #endregion
    }
}