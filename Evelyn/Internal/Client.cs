﻿/*
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
    internal class Client
    {
        private readonly string _clientID;
        private readonly IClientService _service;
        private readonly IAlgorithm? _algorithm;
        private readonly ClientSubscription _subscription;

        public Client(IClientService service, string clientID, IAlgorithm? algorithm = null)
        {
            _algorithm = algorithm;
            _clientID = clientID;
            _service = service;
            _subscription = new ClientSubscription(_clientID);
        }

        public ClientSubscription Subscription => _subscription;

        public string ClientID => _clientID;

        public IAlgorithm Algorithm => _algorithm ?? throw new NoValueException("Client algorithm has no value.");

        public IClientService Service => _service;
    }
}