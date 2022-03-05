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
    internal class EngineBroker
    {
        private IBroker? _broker;
        private BrokerExchange? _brokerExchange;

        private IBroker Broker => _broker ?? throw new NullReferenceException("Broker has no value.");

        internal DateOnly TradingDay => Broker.TradingDay;
        internal string NewOrderID => Broker.NewOrderID;
        internal bool IsConnected => _brokerExchange?.IsConnected ?? throw new NullReferenceException("Borker exchange has no value.");

        internal void Configure(IBroker broker, EngineOrderHandler orderHandler, BrokerExchange exchange)
        {
            _brokerExchange = exchange;
            _broker = broker;
            _broker.Register(orderHandler, exchange);
        }

        internal void Delete(DeleteOrder deleteOrder)
        {
            Broker.Delete(deleteOrder);
        }

        internal void NewOrder(NewOrder newOrder)
        {
            Broker.New(newOrder);
        }
    }
}