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
using System.Timers;

namespace Evelyn.Internal
{
    internal class EngineFeedHandler : IFeedHandler
    {
        private readonly EngineClientHandler _clients;
        private readonly Dictionary<string, Instrument> _instruments = new Dictionary<string, Instrument>();
        private readonly ISet<(Action, string, InstrumentState)> _scheduledJobs = new HashSet<(Action, string, InstrumentState)>();
        private readonly ISet<IOHLCGenerator> _ohlcGenerators = new HashSet<IOHLCGenerator>();

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
                    client.Service.SendTick(tick, client.ClientID);
                }
            });

            foreach(var generator in _ohlcGenerators)
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
                    client.Service.SendOHLC(ohlc, client.ClientID);
                }
            });
        }

        internal void SaveInstrument(Instrument instrument)
        {
            if (!_instruments.TryAdd(instrument.InstrumentID, instrument))
            {
                _instruments[instrument.InstrumentID] = instrument;
            }
        }

        internal void ScheduleOrder(Action action, DateTime moment)
        {
            /*
             * TODO If feed source is running backtest, use ticks to count the time.
             */
            var timer = new System.Timers.Timer(moment.Subtract(DateTime.Now).TotalMilliseconds);

            timer.Elapsed += (object? source, ElapsedEventArgs args) => action();
            timer.AutoReset = false;
            timer.Enabled = true;
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
                    client.Service.SendSubscribe(instrumentID, description, subscribed, client.ClientID);
                    client.Subscription.MarkSubscriptionResponse(instrumentID, waitResponse: false);
                }
            });
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
                    client.Service.SendInstrument(_instruments[instrumentID], client.ClientID);
                }
            });
        }
    }
}
