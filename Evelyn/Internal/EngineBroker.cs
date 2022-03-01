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
using Evelyn.Plugin;

namespace Evelyn.Internal
{
    internal class EngineBroker
    {
        private IBroker? _broker;

        private IBroker Broker => _broker ?? throw new NoValueException("Broker has no value.");

        public bool IsConfigured { get; internal set; } = false;

        public void Configure(IBroker broker)
        {
            _broker = broker;
            IsConfigured = true;
        }

        /// <summary>
        /// Get next broker's order ID. If the given internal order ID is also valid
        /// for a broker's order ID, return the internal order ID.
        /// </summary>
        /// <param name="internalOrderID">Engine's internal order ID.</param>
        /// <returns>Valid broker's order ID.</returns>
        public string NextOrderID(string internalOrderID)
        {
            throw new NotImplementedException();
        }
    }
}