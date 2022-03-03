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
using Microsoft.Extensions.Logging;

namespace Evelyn.Internal.Logging
{
    internal class EvelynLogger : ILogger
    {
        private readonly TextWriter _writer;
        private readonly Stack<LogScope> _scopes;

        internal EvelynLogger(string loggerName, TextWriter? writer = null)
        {
            _writer = writer ?? new StreamWriter(loggerName.Replace('\\', '.').Replace('/', '.') + ".log");
            _scopes = new Stack<LogScope>();
            _scopes.Push(new LogScope(string.Empty, 0, _scopes, _writer));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var scope = new LogScope(state?.ToString() ?? String.Empty, _scopes.Count, _scopes, _writer);
            _scopes.Push(scope);
            return new LogScopeDisposer(scope);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _scopes.Peek().Log(logLevel, eventId, formatter(state, exception));
        }
    }
}
