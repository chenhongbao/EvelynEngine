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
using System.Collections.Concurrent;

namespace Evelyn.Extension.Client
{
    public class AlgorithmClient : IAlgorithm
    {
        private readonly ConcurrentQueue<Filter> _filters = new ConcurrentQueue<Filter>();
        private readonly IAlgorithm _algorithm;

        public string ClientID { get; private init; }
        public string[] InstrumentID { get; private set; } = new string[0];

        public AlgorithmClient(string clientID, IAlgorithm algorithm)
        {
            ClientID = clientID;
            _algorithm = algorithm;
        }

        public AlgorithmClient AddFilter(Filter filter)
        {
            _filters.Enqueue(filter);
            return this;
        }

        public AlgorithmClient Subscribe(params string[] instrumentID)
        {
            InstrumentID = instrumentID;
            return this;
        }

        public void OnFeed(Tick tick)
        {
            bool passed = true;
            if (_filters.Count() == 0 || _filters.Select(filter => passed && (passed = filter.DoFeed(tick))).Last())
            {
                _algorithm.OnFeed(tick);
            }
        }

        public void OnFeed(OHLC ohlc)
        {
            bool passed = true;
            if (_filters.Count() == 0 || _filters.Select(filter => passed && (passed = filter.DoFeed(ohlc))).Last())
            {
                _algorithm.OnFeed(ohlc);
            }
        }

        public void OnFeed(Instrument instrument)
        {
            bool passed = true;
            if (_filters.Count() == 0 || _filters.Select(filter => passed && (passed = filter.DoFeed(instrument))).Last())
            {
                _algorithm.OnFeed(instrument);
            }
        }

        public void OnLoad(IOperator op)
        {
            _algorithm.OnLoad(op);
        }

        public void OnSubscribed(string instrumentID, Description description, bool subscribed)
        {
            _algorithm.OnSubscribed(instrumentID, description, subscribed);
        }

        public void OnTrade(Trade trade, Description description)
        {
            _algorithm.OnTrade(trade, description);
        }

        public void OnUnload()
        {
            _algorithm.OnUnload();
        }
    }

    public static class AlgorithmClientExtensions
    {
        public static IEvelyn RegisterLocalClient(this IEvelyn engine, AlgorithmClient client)
        {
            return engine.RegisterLocalClient(client.ClientID, client, client.InstrumentID);
        }
    }
}
