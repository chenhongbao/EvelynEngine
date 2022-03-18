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

namespace Evelyn.Extension
{
    public class TickRecorder : IOHLCGenerator
    {
        public bool Generate(Tick tick, out OHLC ohlc)
        {
            // TODO Record ticks.

            ohlc = new OHLC();
            return false;
        }
    }

    public static class TickRecorderExtensions
    {
        public static IEvelyn RegisterRecorder(this IEvelyn engine, TickRecorder recorder)
        {
            return engine.GenerateOHLC(recorder);
        }
    }
}
