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
    public class InternalSubscriptionBehaviorVerification
    {
        [TestMethod("Subscribe for an instrument many times.")]
        public void SubscribeManyTimes()
        {
            /*
             * Many clients subscribe for the same instrument and feed source receives
             * subscription request only at the first subscription.
             * 
             * All clients receive subscription response.
             */
        }

        [TestMethod("Unsubscribe an instrument many times until it is removed.")]
        public void UnsubscribeManyTimeUntilRemoved()
        {
            /*
             * Many clients unsubscribe the same instrument and feed source receives
             * the unsubscription request only at the last unsubscription.
             * 
             * All clients receive unsubscription response.
             */
        }

        [TestMethod("Unsubscribe an instrument many times even after it is removed.")]
        public void UnsubscribeManyTimeAfterRemoved()
        {
            /*
             * Unsubscribe a removed instrument sends error response to client.
             */
        }
    }
}
