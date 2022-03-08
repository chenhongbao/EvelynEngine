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

namespace Evelyn.Plugin
{
    public interface IClientService
    {
        public void SendTick(Tick tick, string clientID);

        public void SendOHLC(OHLC ohlc, string clientID);

        public void SendInstrument(Instrument instrument, string clientID);

        public void SendSubscribe(string instrumentID, Description description, bool isSubscribed, string clientID);

        public void SendTrade(Trade trade, Description description, string clientID);

        public void Configure(IClientHandler clientHandler);
    }
}