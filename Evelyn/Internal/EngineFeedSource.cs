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
        private EngineFeedHandler? _feedHandler;
        private FeedSourceExchange? _feedSourceExchange;
        private IFeedSource? _feedSource;
        private IDictionary<string, int> _counters = new Dictionary<string, int>();

        private IFeedSource FeedSource => _feedSource ?? throw new NullReferenceException("Feed source has no value.");

        private EngineFeedHandler FeedHandler => _feedHandler ?? throw new NullReferenceException("Feed handler has no value.");
        private bool IsConnected => _feedSourceExchange?.IsConnected ?? throw new NullReferenceException("Feed source exchange has no value.");

        internal bool IsConfigured { get; private set; } = false;

        internal EngineFeedSource Subscribe()
        {
            _counters.Keys.ToList().ForEach(instrument =>
            {
                FeedSource.Subscribe(instrument);
                FeedHandler.EraseSubscriptionResponse(instrument, isSubscribed: true);
            });
            return this;
        }

        internal EngineFeedSource Subscribe(IEnumerable<string> instruments, bool isSubscribed)
        {
            if (isSubscribed)
            {
                /*
                 * If an instrument has been subscribed, just increase the counter.
                 */
                instruments.Intersect(_counters.Keys).ToList().ForEach(instrument =>
                {
                    ++_counters[instrument];

                    /*
                     * Send a fake unsubscription response to client if the instrument is subscribed
                     * and response has arrived, otherwise waits for response.
                     */
                    if (FeedHandler.HasSubscriptionResponse(instrument, isSubscribed: true))
                    {
                        FeedHandler.OnSubscribed(instrument, new Description { Code = 0, Message = String.Empty }, true);
                    }
                });

                instruments.Except(_counters.Keys).ToList().ForEach(instrument =>
                {
                    if (IsConnected)
                    {
                        FeedSource.Subscribe(instrument);
                        FeedHandler.EraseSubscriptionResponse(instrument, isSubscribed: true);
                    }

                    _counters.Add(instrument, 1);
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
                            if (IsConnected)
                            {
                                FeedSource.Unsubscribe(instrument);
                                FeedHandler.EraseSubscriptionResponse(instrument, isSubscribed: false);
                            }
                            
                            _counters.Remove(instrument);
                        }
                        else
                        {
                            /*
                             * Send a fake unsubscription response to client.
                             * 
                             * NOTE: No need to check unsubscription response here because the real unsubscription
                             * always follows the last unsubscription request, and before the point all fake
                             * responses are correctly sent. After the real unsubscription request is sent to 
                             * feed source, the last client receives the real response.
                             */
                            FeedHandler.OnSubscribed(instrument, new Description { Code = 0, Message = String.Empty }, false);
                        }
                    }
                });
            }

            return this;
        }

        internal void Configure(IFeedSource feedSource, EngineFeedHandler feedHandler, FeedSourceExchange exchange)
        {
            _feedHandler = feedHandler;
            _feedSourceExchange = exchange;
            _feedSource = feedSource;
            _feedSource.Register(feedHandler, exchange);

            IsConfigured = true;
        }
    }
}