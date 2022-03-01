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
namespace Evelyn.Internal
{
    internal class ClientSubscription
    {
        private readonly ISet<string> _instruments;
        private readonly ISet<string> _responses;
        private readonly string _clientID;

        public ClientSubscription(string clientID)
        {
            _clientID = clientID;
            _instruments = new HashSet<string>();
            _responses = new HashSet<string>();
        }

        public string ClientID => _clientID;

        public ISet<string> Instruments => new HashSet<string>(_instruments);

        public void Subscribe(string instrumentID, bool isSubscribed)
        {
            if (isSubscribed)
            {
                _instruments.Add(instrumentID);
            }
            else
            {
                _instruments.Remove(instrumentID);
            }
        }

        internal bool WaitSubscriptionResponse(string instrumentID)
        {
            return _responses.Contains(instrumentID);
        }

        internal void MarkSubscriptionResponse(string instrumentID, bool waitResponse)
        {
            if (waitResponse)
            {
                _responses.Add(instrumentID);
            }
            else
            {
                _responses.Remove(instrumentID);
            }
        }
    }
}