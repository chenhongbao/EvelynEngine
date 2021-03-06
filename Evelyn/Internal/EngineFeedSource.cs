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

namespace Evelyn.Internal
{
    internal class EngineFeedSource
    {
        private readonly ConcurrentDictionary<string, int> _counters = new ConcurrentDictionary<string, int>();

        private EngineFeedHandler? _feedHandler;
        private FeedSourceExchange? _feedSourceExchange;
        private IFeedSource? _feedSource;

        private IFeedSource FeedSource => _feedSource ?? throw new NullReferenceException("Feed source has no value.");
        internal EngineFeedHandler Handler => _feedHandler ?? throw new NullReferenceException("Feed handler has no value.");

        internal bool IsConnected => _feedSourceExchange?.IsConnected ?? throw new NullReferenceException("Feed source exchange has no value.");
        internal DateOnly TradingDay => FeedSource.TradingDay;

        internal EngineFeedSource Subscribe()
        {
            _counters.Keys.ToList().ForEach(instrument =>
            {
                /*
                 * Subscribe for the existing instruments, and clear subcription response marks.
                 */
                Handler.EraseSubscriptionResponse(instrument, isSubscribed: true);
                FeedSource.Subscribe(instrument);
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
                     * Send a fake subscription response to client if the instrument is subscribed
                     * and response has arrived, otherwise waits for response.
                     */
                    if (Handler.HasSubscriptionResponse(instrument, isSubscribed: true))
                    {
                        Handler.OnSubscribed(
                            instrument,
                            new Description
                            {
                                Code = ErrorCodes.OK,
                                Message = String.Empty
                            },
                            true);
                    }
                });

                instruments.Except(_counters.Keys).ToList().ForEach(instrument =>
                {
                    /*
                     * There is a chance that when engine is begine configured, and subscribing instruments,
                     * feed source's exchange is not connecetd yet. So that here it will not make subscription
                     * request to exchange until engine receives exchange connected event.
                     */
                    if (IsConnected)
                    {
                        Handler.EraseSubscriptionResponse(instrument, isSubscribed: true);
                        FeedSource.Subscribe(instrument);
                    }

                    /*
                     * Except operation ensures the instrument doesn't exist in dictionary. So no need
                     * to handle the return value of TryAdd().
                     */
                    _counters.TryAdd(instrument, 1);
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
                            /*
                             * No need to send fake unsubscription response because the real unsubscription to
                             * feed source is sent after the last unsubscription request. And the last client that
                             * unsubscribes an instrument will receives the response from feed source.
                             */
                            if (IsConnected)
                            {
                                Handler.EraseSubscriptionResponse(instrument, isSubscribed: false);
                                FeedSource.Unsubscribe(instrument);
                            }

                            _counters.Remove(instrument, out var _);
                        }
                        else
                        {
                            /*
                             * Send a fake unsubscription response to client if the unsubscribed instrument is still
                             * subscribed by other clients.
                             */
                            Handler.OnSubscribed(
                                instrument,
                                new Description
                                {
                                    Code = ErrorCodes.OK,
                                    Message = String.Empty
                                },
                                false);
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
        }
    }
}