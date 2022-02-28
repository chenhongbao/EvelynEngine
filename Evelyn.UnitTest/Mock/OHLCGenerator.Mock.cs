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
using System;

namespace Evelyn.UnitTest.Mock
{
    internal class MockedOHLCGenerator : IOHLCGenerator
    {
        private bool _hasOHLC = false;
        public OHLC? Generate(Tick tick)
        {
            if (!_hasOHLC)
            {
                _hasOHLC = true;
                return GeneratedOHLC;
            }
            else
            {
                return null;
            }
        }

        internal OHLC GeneratedOHLC { get; init; } = new OHLC { InstrumentID = "l2205", TradingDay = DateOnly.FromDateTime(DateTime.Now), TimeStamp = DateTime.Now, OpenPrice = 5555, HighPrice = 5555, LowPrice = 5555, ClosePrice = 5555, OpenInterest = 55, Volume = 555, Time = TimeSpan.FromMinutes(1) };
    }
}
