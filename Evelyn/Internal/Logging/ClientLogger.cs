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
using Evelyn.Model.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Evelyn.Internal
{
    internal class ClientLogger : ILogger
    {
        internal ConcurrentQueue<ClientLog> Logs { get; init; } = new ConcurrentQueue<ClientLog>();

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotSupportedException("Scope is not supported.");
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logs.Enqueue(new ClientLog { LogLevel = logLevel, LogID = eventId, Message = formatter(state, exception), Timestamp = DateTime.Now });
        }
    }
}