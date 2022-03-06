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
using System.Collections.Concurrent;

namespace Evelyn.Internal
{
    internal class ClientSubscription
    {
        private readonly ConcurrentDictionary<string, bool> _instruments = new ConcurrentDictionary<string, bool>();
        private readonly ConcurrentDictionary<string, bool> _responses = new ConcurrentDictionary<string, bool>();
        private readonly string _clientID;

        internal ClientSubscription(string clientID)
        {
            _clientID = clientID;
        }

        internal List<string> Instruments => new List<string>(_instruments.Keys);

        internal void Subscribe(string instrumentID, bool isSubscribed)
        {
            if (isSubscribed)
            {
                _instruments.TryAdd(instrumentID, default(bool));
            }
            else
            {
                _instruments.Remove(instrumentID, out var _);
            }
        }

        internal bool WaitSubscriptionResponse(string instrumentID)
        {
            return _responses.ContainsKey(instrumentID);
        }

        internal void MarkSubscriptionResponse(string instrumentID, bool waitResponse)
        {
            if (waitResponse)
            {
                _responses.TryAdd(instrumentID, default(bool));
            }
            else
            {
                _responses.Remove(instrumentID, out var _);
            }
        }
    }
}