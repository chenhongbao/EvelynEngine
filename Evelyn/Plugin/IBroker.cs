/*
Null value exception for Evelyn Engine, a quantitative trading engine by Chen Hongbao.
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
namespace PetriSoft.Evelyn.Plugin
{
    public interface IBroker
    {
        /// <summary>
        /// Insert a new single order to broker.
        /// </summary>
        /// <param name="newOrder">New single order.</param>
        /// <param name="orderHandler">Order handler for the given single order.</param>
        public void New(NewOrder newOrder, IOrderHandler orderHandler);

        /// <summary>
        /// Delete an existing order that has not been compeleted.
        /// </summary>
        /// <param name="orderID">Order ID to be delete from broker.</param>
        public void Delete(string orderID);
    }
}
