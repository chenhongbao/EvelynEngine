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
using System.Collections.Concurrent;

namespace Evelyn.Extension.Logging
{
    internal class LogScope : IDisposable
    {
        private readonly string _name;
        private readonly string _loggerName;
        private readonly string _messageIndent;
        private readonly int _scopeLevels;
        private readonly ConcurrentStack<LogScope> _scopes;

        private bool _writeScopeName = false;
        private TextWriter? _writer;

        internal LogScope(string scopeName, int scopeLevels, ConcurrentStack<LogScope> scopes, string loggerName, TextWriter? writer)
        {
            _name = scopeName;
            _loggerName = loggerName;
            _scopes = scopes;
            _scopeLevels = scopeLevels;
            _messageIndent = GetIndent(EvelynLoggerProvider.Indent, scopeLevels);
            _writer = writer;
        }

        public void Dispose()
        {
            if (!_scopes.TryPop(out var top))
            {
                throw new InvalidOperationException("No existing scope to log.");
            }
            if (top != this)
            {
                throw new InvalidOperationException("The scope stack is modified by outside current scope.");
            }
        }

        internal void Log(LogLevel logLevel, EventId eventId, string message)
        {
            if (_writer == null)
            {
                _writer = new StreamWriter(_loggerName.Replace('\\', '.').Replace('/', '.') + "." + GetHashCode() + ".log");
            }

            if (!_writeScopeName && _scopeLevels > 0)
            {
                _writer.WriteLine("#{0}{1}", _messageIndent.Substring(0, _messageIndent.Length - 1), _name);
                _writeScopeName = true;
            }

            _writer.WriteLine(FormatLog(logLevel, eventId, message));
        }

        private string FormatLog(LogLevel logLevel, EventId eventId, string message)
        {
            return _messageIndent + logLevel.ToString() + ", " + eventId.ToString() + ", " + DateTime.Now.ToString() + "\n"
                + _messageIndent + message.Replace("\n", "\n" + _messageIndent);
        }

        private string GetIndent(string indent, int levels)
        {
            var r = "";
            while (levels-- > 0)
            {
                r += indent;
            }
            return r;
        }
    }
}
