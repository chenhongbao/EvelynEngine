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

namespace Evelyn.Extension.Simulator
{
    public class SimulatedBroker : IBroker
    {
        public string NewOrderID => throw new NotImplementedException();

        public DateOnly TradingDay => throw new NotImplementedException();

        public void Delete(DeleteOrder deleteOrder)
        {
            throw new NotImplementedException();
        }

        public void New(NewOrder newOrder)
        {
            throw new NotImplementedException();
        }

        public void Register(IOrderHandler orderHandler, IExchangeListener exchangeListener)
        {
            throw new NotImplementedException();
        }

        internal void Match(Tick tick)
        {

        }
    }
}