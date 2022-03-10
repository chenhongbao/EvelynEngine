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

namespace Evelyn.Extension.Simulator
{
    public class SimulatedFeedSource : IFeedSource
    {
        private readonly SimulatedBroker _broker;
        private readonly IReadOnlyList<FeedEvent> _events;
        private readonly IEnumerator<FeedEvent> _eventEnumerator;
        private readonly ISet<string> _instrumentID = new HashSet<string>();

        public SimulatedFeedSource(SimulatedBroker broker, List<Tick> ticks, List<Instrument> instruments)
        {
            _broker = broker;
            _events = MergeEvents(ticks, instruments);
            _eventEnumerator = _events.GetEnumerator();
        }

        private IReadOnlyList<FeedEvent> MergeEvents(List<Tick> ticks, List<Instrument> instruments)
        {
            ticks.Select(tick => tick.InstrumentID).Distinct()
                .Union(instruments.Select(instrument => instrument.InstrumentID).Distinct()).Distinct()
                .ToList().ForEach(instrumentID => _instrumentID.Add(instrumentID));

            var events = new List<FeedEvent>();

            events.AddRange(ticks.Select(tick => new FeedEvent
            {
                Object = tick,
                Type = typeof(Tick),
                TimeStamp = tick.TimeStamp
            }).ToList());
            events.AddRange(instruments.Select(instrument => new FeedEvent
            {
                Object = instrument,
                Type = typeof(Instrument),
                TimeStamp = instrument.EnterTime
            }).ToList());

            events.Sort((a, b) => a.TimeStamp.CompareTo(b.TimeStamp));

            return events;
        }

        public DateOnly TradingDay => throw new NotImplementedException();

        public void Register(IFeedHandler feedHandler, IExchangeListener exchangeListener)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string instrumentID)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(string instrumentID)
        {
            throw new NotImplementedException();
        }

        public bool Flip()
        {
            lock (_events)
            {
                var r = _eventEnumerator.MoveNext();
                if (r)
                {
                    /*
                     * Call back.
                     */
                }
                else
                {
                    _eventEnumerator.Reset();
                }

                return r;
            }
        }
    }
}