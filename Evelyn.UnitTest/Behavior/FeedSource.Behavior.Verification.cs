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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evelyn.UnitTest.Behavior
{
    [TestClass]
    public class FeedSourceBehaviorVerification
    {
        [TestMethod("Subscribe for an instrument many times.")]
        public void SubscribeManyTimes()
        {
            /*
             * There are many clients subscribing for the same instrument.
             * 
             * 1. Client A subscribes for the instrument.
             * 2. Feed source sends market data and client A receives data.
             * 3. Client B subscribes for the same instrument.
             * 4. Feed source sends the market data and both clients receive data.
             */
        }

        [TestMethod("Unsubscribe for an instrument many times.")]
        public void UnsubscribeManyTimes()
        {
            /*
             * There are many clients subscribing for the same instrument, and then
             * unsubscribe it.
             * 
             * 1. Client A and B both subscribe for an instrument.
             * 2. Feed source sends market data and both A and B receive the data.
             * 3. Client A unsubscribes the instrument.
             * 4. Client A receives no data, while client B still receives data.
             * 5. Client B unsubscribes the instrument, and none of the clients receives data anmy more.
             */
        }
    }
}
