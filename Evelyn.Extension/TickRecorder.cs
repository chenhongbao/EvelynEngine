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

        public static bool ParseTick(string line, out Tick tick)
        {
            tick = new Tick();
            var splits = line.Split(",");
            if (splits.Length != 21)
            {
                return false;
            }
            else
            {
                tick.InstrumentID = splits[0];
                tick.ExchangeID = splits[1];
                tick.Symbol = splits[2];
                tick.TradingDay = ParseDate(splits[3]);
                tick.TimeStamp = ParseDateTime(splits[4]);
                tick.AveragePrice = ParseDouble(splits[5]);
                tick.LastPrice = ParseDouble(splits[6]) ?? throw new InvalidDataException("Invalid last price " + splits[6] + ".");
                tick.OpenPrice = ParseDouble(splits[7]);
                tick.HighPrice = ParseDouble(splits[8]);
                tick.LowPrice = ParseDouble(splits[9]);
                tick.ClosePrice = ParseDouble(splits[10]);
                tick.SettlementPrice = ParseDouble(splits[11]);
                tick.Volume = ParseLong(splits[12]);
                tick.OpenInterest = ParseLong(splits[13]);
                tick.PreClosePrice = ParseDouble(splits[14]) ?? throw new InvalidDataException("Invalid pre close price " + splits[14] + ".");
                tick.PreSettlementPrice = ParseDouble(splits[15]) ?? throw new ArgumentException("Invalid pre settlement price " + splits[15] + ".");
                tick.PreOpenInterest = ParseLong(splits[16]);
                tick.AskPrice = ParseDouble(splits[17]) ?? throw new InvalidDataException("Invalid ask price " + splits[17] + ".");
                tick.AskVolume = ParseLong(splits[18]);
                tick.BidPrice = ParseDouble(splits[19]) ?? throw new InvalidDataException("Invalid bid price " + splits[18] + ".");
                tick.BidVolume = ParseLong(splits[20]);

                return true;
            }
        }

        public static string Format(Tick tick)
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}",
                tick.InstrumentID, tick.ExchangeID, tick.Symbol, DateFormat(tick.TradingDay), DateTimeFormat(tick.TimeStamp),
                ValidOrNaN(tick.AveragePrice), ValidOrNaN(tick.LastPrice), ValidOrNaN(tick.OpenPrice), ValidOrNaN(tick.HighPrice), ValidOrNaN(tick.LowPrice),
                ValidOrNaN(tick.ClosePrice), ValidOrNaN(tick.SettlementPrice), ValidOrNaN(tick.Volume), ValidOrNaN(tick.OpenInterest), ValidOrNaN(tick.PreClosePrice),
                ValidOrNaN(tick.PreSettlementPrice), ValidOrNaN(tick.PreOpenInterest), ValidOrNaN(tick.AskPrice), ValidOrNaN(tick.AskVolume), ValidOrNaN(tick.BidPrice),
                ValidOrNaN(tick.BidVolume));
        }

        internal static long ParseLong(string value)
        {
            return long.Parse(value);
        }

        internal static DateTime ParseDateTime(string datetime)
        {
            return DateTime.ParseExact(datetime, "yyyyMMdd HH:mm:ss.fffffff", null);
        }

        internal static DateOnly ParseDate(string date)
        {
            return DateOnly.ParseExact(date, "yyyyMMdd");
        }

        internal static double? ParseDouble(string value)
        {
            if (value == "NaN")
            {
                return null;
            }
            else
            {
                return double.Parse(value);
            }
        }

        internal static string ValidOrNaN(double? value)
        {
            if (value == null)
            {
                return "NaN";
            }
            else
            {
                return string.Format("{0:F3}", value);
            }
        }

        internal static string ValidOrNaN(long? value)
        {
            return value.ToString() ?? "NaN";
        }

        internal static string DateFormat(DateOnly value)
        {
            return string.Format("{0:yyyyMMdd}", value);
        }

        internal static string DateTimeFormat(DateTime value)
        {
            return string.Format("{0:yyyyMMdd HH:mm:ss.fffffff}", value);
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
