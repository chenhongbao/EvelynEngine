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
using System.Collections.Concurrent;

namespace Evelyn.Internal
{
    internal class EngineFeedHandler : IFeedHandler
    {
        private readonly EngineClientHandler _clientHandler;
        private readonly ConcurrentBag<IOHLCGenerator> _ohlcGenerators = new ConcurrentBag<IOHLCGenerator>();
        private readonly ConcurrentDictionary<string, bool> _subscriptionResponses = new ConcurrentDictionary<string, bool>();
        private readonly ConcurrentDictionary<string, Instrument> _instruments = new ConcurrentDictionary<string, Instrument>();
        private readonly ConcurrentDictionary<string, ScheduledJob> _scheduledJobs = new ConcurrentDictionary<string, ScheduledJob>();
        private int _jobCounter = 0;

        private ILogger Logger { get; } = Loggers.CreateLogger(nameof(EngineFeedHandler));

        internal IReadOnlyCollection<IOHLCGenerator> OHLCGenerators => _ohlcGenerators;
        internal IReadOnlyDictionary<string, bool> SubscriptionResponses => _subscriptionResponses;
        internal IReadOnlyDictionary<string, Instrument> Instruments => _instruments;
        internal IReadOnlyDictionary<string, ScheduledJob> ScheduledJobs => _scheduledJobs;

        internal EngineFeedHandler(EngineClientHandler clientHandler)
        {
            _clientHandler = clientHandler;
        }

        public void OnFeed(Tick tick)
        {
            foreach (var client in _clientHandler.Clients.Values)
            {
                if (client.Subscription.Instruments.Contains(tick.InstrumentID))
                {
                    TryCatch(() => client.Service.SendTick(tick, client.ClientID));
                }
            }

            foreach (var generator in _ohlcGenerators)
            {
                TryCatch(() =>
                {
                    if (generator.Generate(tick, out OHLC ohlc))
                    {
                        OnFeed(ohlc);
                    }
                });
            }

            /*
             * Check and run time scheduled jobs.
             */
            CheckRunScheduledJobs();
        }

        public void OnFeed(OHLC ohlc)
        {
            foreach (var client in _clientHandler.Clients.Values)
            {
                if (client.Subscription.Instruments.Contains(ohlc.InstrumentID))
                {
                    TryCatch(() => client.Service.SendOHLC(ohlc, client.ClientID));
                }
            }
        }

        internal bool HasSubscriptionResponse(string instrument, bool isSubscribed)
        {
            return _subscriptionResponses.TryGetValue(instrument, out bool subscriptionType) && subscriptionType == isSubscribed;
        }

        internal void EraseSubscriptionResponse(string instrument, bool isSubscribed)
        {
            if (HasSubscriptionResponse(instrument, isSubscribed))
            {
                _subscriptionResponses.Remove(instrument, out var _);
            }
        }

        internal void SaveInstrument(Instrument instrument)
        {
            /*
             * The updateValueFactory takes key's existing value as the second parameter.
             * So don't use the second parameter as the factory's return value.
             */
            _instruments.AddOrUpdate(instrument.InstrumentID, instrument, (_, _) => instrument);
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

        internal void ScheduleJob(string name, Action job, string instrumentID, OrderOption option)
        {
            if (CheckOrderOption(instrumentID, option))
            {
                job();
            }
            else
            {
                _scheduledJobs.TryAdd(Guid.NewGuid().ToString(),
                    new ScheduledJob
                    {
                        JobID = Interlocked.Increment(ref _jobCounter),
                        Name = name,
                        Job = job,
                        InstrumentID = instrumentID,
                        Option = option,
                        SchedulingTime = DateTime.Now,
                    });
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
                && _instruments.TryGetValue(instrumentID, out var instrument) && instrument.Status == option.Trigger.StateChange)
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
            foreach (var client in _clientHandler.Clients.Values)
            {
                if (client.Subscription.WaitSubscriptionResponse(instrumentID))
                {
                    TryCatch(() =>
                    {
                        client.Service.SendSubscribe(instrumentID, description, subscribed, client.ClientID);
                        client.Subscription.MarkSubscriptionResponse(instrumentID, waitResponse: false);
                    });
                }
            }

            /*
             * Erase old state and mark the response received.
             */
            EraseSubscriptionResponse(instrumentID, isSubscribed: !subscribed);
            _subscriptionResponses.AddOrUpdate(instrumentID, subscribed, (_, _) => subscribed);
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
            foreach (var client in _clientHandler.Clients.Values)
            {
                var instrumentID = instrument.InstrumentID;
                if (client.Subscription.Instruments.Contains(instrumentID))
                {
                    TryCatch(() => client.Service.SendInstrument(_instruments[instrumentID], client.ClientID));
                }
            }
        }
    }
}
