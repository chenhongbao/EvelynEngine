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
    public class FeedRecorder : IAlgorithm
    {
        public void OnFeed(Tick tick)
        {
            using (StreamWriter sw = new StreamWriter(GetFile(tick.InstrumentID, "tick"), append: true))
            {
                sw.WriteLine("{0}", Format(tick));
            }
        }

        public void OnFeed(OHLC ohlc)
        {
            using (StreamWriter sw = new StreamWriter(GetFile(ohlc.InstrumentID, "ohlc"), append: true))
            {
                sw.WriteLine("{0}", Format(ohlc));
            }
        }

        public void OnFeed(Instrument instrument)
        {
            using (StreamWriter sw = new StreamWriter(GetFile(instrument.InstrumentID, "inst"), append: true))
            {
                sw.WriteLine("{0}", Format(instrument));
            }
        }

        public static string Format(OHLC ohlc)
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                ohlc.InstrumentID, ohlc.ExchangeID, ohlc.Symbol, Format(ohlc.TradingDay), Format(ohlc.TimeStamp),
                Format(ohlc.OpenPrice), Format(ohlc.HighPrice), Format(ohlc.LowPrice), Format(ohlc.ClosePrice), Format(ohlc.OpenInterest),
                Format(ohlc.Volume), Format(ohlc.Time));
        }

        public static string Format(Instrument instrument)
        {
            return string.Format("{0},{1},{2},{3},{4},{5}",
                instrument.InstrumentID, instrument.ExchangeID, instrument.Symbol, Format(instrument.TradingDay), Format(instrument.Status),
                Format(instrument.EnterTime));
        }

        internal static string Format(InstrumentStatus status)
        {
            return status.ToString();
        }

        public static bool Parse(string line, out Instrument instrument)
        {
            instrument = new Instrument();
            var splits = line.Split(",");
            if (splits.Length != 6)
            {
                return false;
            }
            else
            {
                instrument.InstrumentID = splits[0];
                instrument.ExchangeID = splits[1];
                instrument.Symbol = splits[2];
                instrument.TradingDay = ParseDate(splits[3]);
                instrument.Status = ParseInstrumentStatus(splits[4]);
                instrument.EnterTime = ParseDateTime(splits[5]);

                return true;
            }
        }

        internal static InstrumentStatus ParseInstrumentStatus(string value)
        {
            if (value == InstrumentStatus.BeforeTrading.ToString())
            {
                return InstrumentStatus.BeforeTrading;
            }
            else if (value == InstrumentStatus.NoTrading.ToString())
            {
                return InstrumentStatus.NoTrading;
            }
            else if (value == InstrumentStatus.AuctionOrdering.ToString())
            {
                return InstrumentStatus.AuctionOrdering;
            }
            else if (value == InstrumentStatus.AuctionBalance.ToString())
            {
                return InstrumentStatus.AuctionBalance;
            }
            else if (value == InstrumentStatus.AuctionMatch.ToString())
            {
                return InstrumentStatus.AuctionMatch;
            }
            else if (value == InstrumentStatus.Continous.ToString())
            {
                return InstrumentStatus.Continous;
            }
            else if (value == InstrumentStatus.Closed.ToString())
            {
                return InstrumentStatus.Closed;
            }
            else
            {
                return (InstrumentStatus)0;
            }
        }

        public static bool Parse(string line, out OHLC ohlc)
        {
            ohlc = new OHLC();
            var splits = line.Split(",");
            if (splits.Length != 12)
            {
                return false;
            }
            else
            {
                ohlc.InstrumentID = splits[0];
                ohlc.ExchangeID = splits[1];
                ohlc.Symbol = splits[2];
                ohlc.TradingDay = ParseDate(splits[3]);
                ohlc.TimeStamp = ParseDateTime(splits[4]);
                ohlc.OpenPrice = ParseDouble(splits[5]) ?? throw new InvalidDataException("Invalid open price " + splits[5] + ".");
                ohlc.HighPrice = ParseDouble(splits[6]) ?? throw new InvalidDataException("Invalid high price " + splits[6] + ".");
                ohlc.LowPrice = ParseDouble(splits[7]) ?? throw new InvalidDataException("Invalid low price " + splits[7] + ".");
                ohlc.ClosePrice = ParseDouble(splits[8]) ?? throw new InvalidDataException("Invalid close price " + splits[8] + ".");
                ohlc.OpenInterest = ParseLong(splits[9]);
                ohlc.Volume = ParseLong(splits[10]);
                ohlc.Time = ParseTimeSpan(splits[11]);

                return true;
            }
        }

        internal static TimeSpan ParseTimeSpan(string value)
        {
            return TimeSpan.FromSeconds(ParseLong(value));
        }

        internal static string Format(TimeSpan span)
        {
            return span.TotalSeconds.ToString();
        }

        public static bool Parse(string line, out Tick tick)
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
                tick.AskPrice = ParseDouble(splits[17]);
                tick.AskVolume = ParseLong(splits[18]);
                tick.BidPrice = ParseDouble(splits[19]);
                tick.BidVolume = ParseLong(splits[20]);

                return true;
            }
        }

        public static string Format(Tick tick)
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}",
                tick.InstrumentID, tick.ExchangeID, tick.Symbol, Format(tick.TradingDay), Format(tick.TimeStamp),
                Format(tick.AveragePrice), Format(tick.LastPrice), Format(tick.OpenPrice), Format(tick.HighPrice), Format(tick.LowPrice),
                Format(tick.ClosePrice), Format(tick.SettlementPrice), Format(tick.Volume), Format(tick.OpenInterest), Format(tick.PreClosePrice),
                Format(tick.PreSettlementPrice), Format(tick.PreOpenInterest), Format(tick.AskPrice), Format(tick.AskVolume), Format(tick.BidPrice),
                Format(tick.BidVolume));
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

        internal static string Format(double? value)
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

        internal static string Format(long? value)
        {
            return value.ToString() ?? "NaN";
        }

        internal static string Format(DateOnly value)
        {
            return string.Format("{0:yyyyMMdd}", value);
        }

        internal static string Format(DateTime value)
        {
            return string.Format("{0:yyyyMMdd HH:mm:ss.fffffff}", value);
        }

        private string GetFile(string instrumentID, string type)
        {
            var directory = Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, ".Recorder", "Feed"));
            return Path.Combine(directory.FullName, instrumentID + "." + type + ".txt");
        }

        private bool GenerateNone(out OHLC ohlc)
        {
            ohlc = new OHLC();
            return false;
        }

        public void OnLoad(IOperator op)
        {
        }

        public void OnUnload()
        {
        }

        public void OnSubscribed(string instrumentID, Description description, bool subscribed)
        {
        }

        public void OnTrade(Trade trade, Description description)
        {
        }
    }

    public static class TickRecorderExtensions
    {
        public static IEvelyn RegisterRecorder(this IEvelyn engine, FeedRecorder recorder, params string[] instrumentID)
        {
            return engine.RegisterLocalClient("FEED_RECORDER_" + Guid.NewGuid().ToString("N"), recorder, instrumentID);
        }
    }
}
