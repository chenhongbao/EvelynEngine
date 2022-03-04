﻿/*
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
using System.Collections.Concurrent;
using System.Timers;

namespace Evelyn.Internal
{
    internal class EngineFeedHandler : IFeedHandler
    {
        private readonly EngineClientHandler _clients;
        private readonly Dictionary<string, Instrument> _instruments = new Dictionary<string, Instrument>();
        private readonly ISet<IOHLCGenerator> _ohlcGenerators = new HashSet<IOHLCGenerator>();
        private readonly ISet<(string, bool)> _subscriptionResponses = new HashSet<(string, bool)>();

        private readonly ConcurrentDictionary<string, ScheduledJob> _scheduledJobs = new ConcurrentDictionary<string, ScheduledJob>();

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

            /*
             * Check and run time scheduled jobs.
             */
            CheckRunScheduledJobs();
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

        internal void ScheduleOrder(Action job, string instrumentID, OrderOption option)
        {
            if (CheckOrderOption(instrumentID, option))
            {
                job();
            }
            else
            {
                _scheduledJobs.TryAdd(Guid.NewGuid().ToString(), new ScheduledJob { Job = job, InstrumentID = instrumentID, Option = option });
            }
        }

        private bool CheckOrderOption(string instrumentID, OrderOption option)
        {
            if (option.Trigger.When == TriggerType.Immediate)
            {
                return true;
            }
            else if (option.Trigger.When == TriggerType.Time
                && option.Trigger.Time.CompareTo(DateTime.Now) < 0)
            {
                return true;
            }
            else if (option.Trigger.When == TriggerType.StateChange
                && _instruments.TryGetValue(instrumentID, out var instrument) && instrument.State == option.Trigger.StateChange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CheckRunScheduledJobs()
        {
            var removed = new List<string>();
            foreach (var entry in _scheduledJobs)
            {
                var scheduled = entry.Value;
                if (CheckOrderOption(scheduled.InstrumentID, scheduled.Option))
                {
                    removed.Add(entry.Key);
                    TryCatch(() => scheduled.Job());
                }
            }

            removed.ForEach(jobID => _scheduledJobs.Remove(jobID, out var _));
        }

        public void OnInstrument(Instrument instrument)
        {
            /*
             * Save and send the instrument.
             */
            SaveInstrument(instrument);
            SendInstrument(instrument);

            /*
             * Check and run scheduled jobs of state condition.
             */
            CheckRunScheduledJobs();
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
