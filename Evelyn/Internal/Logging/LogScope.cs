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

#if DEBUG
#endif

namespace Evelyn.Internal.Logging
{
    internal class LogScope : IDisposable
    {
        private readonly int _scopeLevels;
        private readonly string _name;
        private readonly LogWriter _writer;
        private readonly Stack<LogScope> _scopes;
        private readonly Queue<string> _logs = new Queue<string>();
        private readonly DateTime _initTime = DateTime.Now;

        internal LogScope(string scopeName, int scopeLevels, Stack<LogScope> scopes, LogWriter writer)
        {
            _name = scopeName;
            _scopeLevels = scopeLevels;
            _scopes = scopes;
            _writer = writer;
        }

        public void Dispose()
        {
            _writer.WriteLine("{0}Scope: {1}, From: {2}, To: {3}", 
                new String(Loggers.Indent, _scopeLevels), _name, _initTime.ToString(), DateTime.Now.ToString());
            _logs.ToList().ForEach(log => _writer.WriteLine(log.Replace("\n", "\n" + new String(Loggers.Indent, _scopeLevels))));
            _logs.Clear();

            var top = _scopes.Pop();
            if (top != this)
            {
                throw new InvalidOperationException("The scope stack is modified by outside current scope.");
            }
        }

        internal void Keep(LogLevel logLevel, EventId eventId, string message)
        {
            if (_scopeLevels == 0)
            {
                _writer.WriteLine(FormatLog(logLevel, eventId, message));
            }
            else
            {
                _logs.Enqueue(FormatLog(logLevel, eventId, message));
            }
        }

        private string FormatLog(LogLevel logLevel, EventId eventId, string message)
        {
            return logLevel.ToString() + ", " + eventId.Name + "(" + eventId.Id + ")\n" + message;
        }
    }
}
