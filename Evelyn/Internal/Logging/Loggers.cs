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
using Microsoft.Extensions.Logging;

#if DEBUG
using Microsoft.Extensions.Logging.Debug;
#endif

namespace Evelyn.Internal.Logging
{
    public class Loggers
    {
        public static string Indent { get; set; } = "\u0020\u0020\u0020\u0020";

        public static TextWriter? Writer = null;

        internal static ILogger CreateLogger(string loggerName)
        {
#if DEBUG
            return new DebugLoggerProvider().CreateLogger(loggerName);
#else
            return new EvelynLogger(loggerName, Writer);
#endif
        }
    }
}
