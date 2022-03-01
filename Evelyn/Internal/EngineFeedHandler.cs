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
    internal class EngineFeedHandler : IFeedHandler
    {
        private readonly EngineClientHandler _clients;

        public EngineFeedHandler(EngineClientHandler clientHandler)
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

        public void OnInstrument(Instrument instrument)
        {
            _clients.Clients.ForEach(client =>
            {
                if (client.Subscription.Instruments.Contains(instrument.InstrumentID))
                {
                    client.Service.SendInstrument(instrument, client.ClientID);
                }
            });
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
    }
}
