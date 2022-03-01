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

namespace Evelyn.Internal
{
    internal class EngineFeedSource
    {
        private readonly EngineFeedHandler _feedHandler;
        private IFeedSource? _feedSource;
        private IDictionary<string, int> _counters = new Dictionary<string, int>();

        private IFeedSource FeedSource => _feedSource ?? throw new NoValueException("Feed source has no value.");

        public bool IsConfigured { get; internal set; } = false;

        public EngineFeedSource(EngineFeedHandler feedHandler)
        {
            _feedHandler = feedHandler;
        }

        internal EngineFeedSource Subscribe(IEnumerable<string> instruments, bool isSubscribed)
        {
            if (isSubscribed)
            {
                instruments.Except(_counters.Keys).ToList().ForEach(instrument =>
                {
                    FeedSource.Subscribe(instrument, _feedHandler);
                    _counters.Add(instrument, 1);
                });

                /*
                 * If an instrument has been subscribed, just increase the counter.
                 */
                instruments.Intersect(_counters.Keys).ToList().ForEach(instrument =>
                {
                    ++_counters[instrument];

                    /*
                     * Send a fake unsubscription response to client.
                     */
                    _feedHandler.OnSubscribed(instrument, new Description { Code = 0, Message = "OK" }, true);
                });
            }
            else
            {
                instruments.ToList().ForEach(instrument =>
                {
                    if (_counters.ContainsKey(instrument))
                    {
                        --_counters[instrument];
                        if (_counters[instrument] == 0)
                        {
                            FeedSource.Unsubscribe(instrument);
                            _counters.Remove(instrument);
                        }
                        else
                        {
                            /*
                             * Send a fake unsubscription response to client.
                             */
                            _feedHandler.OnSubscribed(instrument, new Description { Code = 0, Message = "OK" }, false);
                        }
                    }
                });
            }

            return this;
        }

        internal void Configure(IFeedSource feedSource)
        {
            _feedSource = feedSource;
            IsConfigured = true;
        }
    }
}