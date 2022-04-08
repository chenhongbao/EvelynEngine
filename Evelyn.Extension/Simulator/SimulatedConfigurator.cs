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
    public class SimulatedConfigurator : IConfigurator
    {
        private readonly List<Tick> _ticks = new List<Tick>();
        private readonly List<OHLC> _ohlcs = new List<OHLC>();
        private readonly List<Instrument> _instruments = new List<Instrument>();
        private SimulatedFeedSource? _feedSource;

        public SimulatedConfigurator(List<Tick> ticks, List<Instrument> instruments)
        {
            _ticks.AddRange(ticks);
            _instruments.AddRange(instruments);
        }

        public SimulatedConfigurator(List<OHLC> ohlcs, List<Instrument> instruments)
        {
            _ohlcs.AddRange(ohlcs);
            _instruments.AddRange(instruments);
        }

        public void Configure(out IBroker broker, out IFeedSource feedSource)
        {
            broker = new SimulatedBroker();

            _feedSource = new SimulatedFeedSource((SimulatedBroker)broker, _ticks, _ohlcs, _instruments);
            feedSource = _feedSource;
        }

        public SimulatedFeedSource FeedSource => _feedSource ?? throw new NullReferenceException("Feed source has no value.");
    }
}
