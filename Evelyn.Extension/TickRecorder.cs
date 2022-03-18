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
using System.Collections.Concurrent;

namespace Evelyn.Extension
{
    public class TickRecorder : IOHLCGenerator
    {
        public readonly ConcurrentDictionary<string, FileStream> _fileStream = new ConcurrentDictionary<string, FileStream>();

        public bool Generate(Tick tick, out OHLC ohlc)
        {
            using (StreamWriter sw = new StreamWriter(_fileStream.GetOrAdd(tick.InstrumentID, instrumentID => GetFileStream(instrumentID))))
            {
                sw.WriteLine("{0}", Format(tick));
            }

            return GenerateNone(out ohlc);
        }

        private string Format(Tick tick)
        {
            // TODO Format tick into a line.
            return string.Empty;
        }

        private FileStream GetFileStream(string instrumentID)
        {
            var directory = Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, ".Recorder", "Tick"));
            return new FileStream(Path.Combine(directory.FullName, instrumentID + ".txt"), FileMode.Append, FileAccess.Write);
        }

        private bool GenerateNone(out OHLC ohlc)
        {
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
