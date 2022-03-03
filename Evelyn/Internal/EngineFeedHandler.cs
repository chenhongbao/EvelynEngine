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
using Evelyn.Internal.Logging;
using Evelyn.Model;
using Evelyn.Plugin;
using Microsoft.Extensions.Logging;
using System.Timers;

namespace Evelyn.Internal
{
    internal class EngineFeedHandler : IFeedHandler
    {
        private readonly EngineClientHandler _clients;
        private readonly Dictionary<string, Instrument> _instruments = new Dictionary<string, Instrument>();
        private readonly ISet<(Action, string, InstrumentState)> _scheduledJobs = new HashSet<(Action, string, InstrumentState)>();
        private readonly ISet<IOHLCGenerator> _ohlcGenerators = new HashSet<IOHLCGenerator>();
        private readonly ISet<(string, bool)> _subscriptionResponses = new HashSet<(string, bool)>();

        private ILogger Logger { get; } = Loggers.CreateLogger(nameof(EngineFeedHandler));

        internal EngineFeedHandler(EngineClientHandler clientHandler)
        {
            _clients = clientHandler;
        }

        public void OnFeed(Tick tick)
        {
            _clients.Clients.ForEach(client =>
            {
                if (client.Subscription.Instruments.Contains(tick.InstrumentID))
                {
                    TryCatch(() => client.Service.SendTick(tick, client.ClientID));
                }
            });

            foreach (var generator in _ohlcGenerators)
            {
                if (generator.Generate(tick, out OHLC ohlc))
                {
                    OnFeed(ohlc);
                }
            }
        }

        public void OnFeed(OHLC ohlc)
        {
            _clients.Clients.ForEach(client =>
            {
                if (client.Subscription.Instruments.Contains(ohlc.InstrumentID))
                {
                    TryCatch(() => client.Service.SendOHLC(ohlc, client.ClientID));
                }
            });
        }

        internal bool HasSubscriptionResponse(string instrument, bool isSubscribed)
        {
            var enumerator = _subscriptionResponses.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var response = enumerator.Current;
                if (response.Item1 == instrument && response.Item2 == isSubscribed)
                {
                    return true;
                }
            }
            return false;
        }

        internal void EraseSubscriptionResponse(string instrument, bool isSubscribed)
        {
            (string, bool) removed = default;

            var enumerator = _subscriptionResponses.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var response = enumerator.Current;
                if (response.Item1 == instrument && response.Item2 == isSubscribed)
                {
                    removed = response;
                    break;
                }
            }

            if (removed != default)
            {
                _subscriptionResponses.Remove(removed);
            }
        }

        internal void SaveInstrument(Instrument instrument)
        {
            if (!_instruments.TryAdd(instrument.InstrumentID, instrument))
            {
                _instruments[instrument.InstrumentID] = instrument;
            }
        }

        internal void ScheduleOrder(Action action, DateTime time)
        {
            /*
             * TODO If feed source is running backtest, use ticks to count the time.
             */
            if (DateTime.Now.CompareTo(time) > 0)
            {
                /*
                 * The given moment has elapsed, do the job now.
                 */
                TryCatch(action);
            }
            else
            {
                var timer = new System.Timers.Timer(time.Subtract(DateTime.Now).TotalMilliseconds);

                timer.Elapsed += (object? source, ElapsedEventArgs args) => TryCatch(action);
                timer.AutoReset = false;
                timer.Enabled = true;
            }
        }

        private void TryCatch(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.LogWarning("{0}\n{1}", ex.Message, ex.StackTrace?.ToString());
            }
        }

        internal void ScheduleOrder(Action job, string instrumentID, InstrumentState state)
        {
            if (_instruments.TryGetValue(instrumentID, out var instrument) && instrument.State == state)
            {
                job();
            }
            else
            {
                _scheduledJobs.Add((job, instrumentID, state));
            }
        }

        public void OnInstrument(Instrument instrument)
        {
            /*
             * Save and send the instrument.
             */
            SaveInstrument(instrument);
            SendInstrument(instrument);

            /*
             * Check scheduled jobs and run if it meets the condition.
             */
            var enumerator = _scheduledJobs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var job = enumerator.Current;

                if (job.Item2 == instrument.InstrumentID && job.Item3 == instrument.State)
                {
                    job.Item1();
                }
            }
        }

        public void OnSubscribed(string instrumentID, Description description, bool subscribed)
        {
            _clients.Clients.ForEach(client =>
            {
                if (client.Subscription.WaitSubscriptionResponse(instrumentID))
                {
                    TryCatch(() =>
                    {
                        client.Service.SendSubscribe(instrumentID, description, subscribed, client.ClientID);
                        client.Subscription.MarkSubscriptionResponse(instrumentID, waitResponse: false);
                    });
                }
            });

            /*
             * Mark the response received, and erase the old state.
             */
            _subscriptionResponses.Add((instrumentID, subscribed));
            EraseSubscriptionResponse(instrumentID, isSubscribed: !subscribed);
        }

        internal void SendInstruments()
        {
            _instruments.Values.ToList().ForEach(instrument => SendInstrument(instrument));
        }

        internal void RegisterOHLCGenerator(IOHLCGenerator generator)
        {
            _ohlcGenerators.Add(generator);
        }

        private void SendInstrument(Instrument instrument)
        {
            _clients.Clients.ForEach(client =>
            {
                var instrumentID = instrument.InstrumentID;
                if (client.Subscription.Instruments.Contains(instrumentID))
                {
                    TryCatch(() => client.Service.SendInstrument(_instruments[instrumentID], client.ClientID));
                }
            });
        }
    }
}
