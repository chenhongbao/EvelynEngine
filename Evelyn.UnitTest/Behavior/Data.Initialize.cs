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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evelyn.UnitTest.Behavior
{
    public class DataInitialize
    {
        internal List<Tick> MockedTicks { get; private set; } = new List<Tick>();
        internal List<OHLC> MockedOHLCs { get; private set; } = new List<OHLC>();
        internal List<Instrument> MockedInstruments { get; private set; } = new List<Instrument>();

        public void Initialize()
        {
            /*
             * Initialize mocked data.
             */
            var baseTime = DateTime.Now;
            var baseDay = DateOnly.FromDateTime(baseTime);

            MockedTicks.Clear();
            MockedOHLCs.Clear();
            MockedInstruments.Clear();

            /*
             * 1. Create mocked ticks.
             */
            for (var count = 0; count < 2; ++count)
            {
                MockedTicks.Add(new Tick { InstrumentID = "l2205", TradingDay = baseDay, TimeStamp = baseTime.AddSeconds(count * 6), LastPrice = 5555, Volume = 555, OpenInterest = 55, PreClosePrice = 5555, PreSettlementPrice = 5555, PreOpenInterest = 55, AskPrice = 5555, AskVolume = 555, BidPrice = 5554, BidVolume = 554 });
                MockedTicks.Add(new Tick { InstrumentID = "l2205", TradingDay = baseDay, TimeStamp = baseTime.AddSeconds(count * 6 + 1), LastPrice = 5555, Volume = 555, OpenInterest = 55, PreClosePrice = 5555, PreSettlementPrice = 5555, PreOpenInterest = 55, AskPrice = 5555, AskVolume = 555, BidPrice = 5554, BidVolume = 554 });
                MockedTicks.Add(new Tick { InstrumentID = "pp2205", TradingDay = baseDay, TimeStamp = baseTime.AddSeconds(count * 6 + 2), LastPrice = 5555, Volume = 555, OpenInterest = 55, PreClosePrice = 5555, PreSettlementPrice = 5555, PreOpenInterest = 55, AskPrice = 5555, AskVolume = 555, BidPrice = 5554, BidVolume = 554 });
                MockedTicks.Add(new Tick { InstrumentID = "l2205", TradingDay = baseDay, TimeStamp = baseTime.AddSeconds(count * 6 + 3), LastPrice = 5555, Volume = 555, OpenInterest = 55, PreClosePrice = 5555, PreSettlementPrice = 5555, PreOpenInterest = 55, AskPrice = 5555, AskVolume = 555, BidPrice = 5554, BidVolume = 554 });
                MockedTicks.Add(new Tick { InstrumentID = "pp2205", TradingDay = baseDay, TimeStamp = baseTime.AddSeconds(count * 6 + 4), LastPrice = 5555, Volume = 555, OpenInterest = 55, PreClosePrice = 5555, PreSettlementPrice = 5555, PreOpenInterest = 55, AskPrice = 5555, AskVolume = 555, BidPrice = 5554, BidVolume = 554 });
                MockedTicks.Add(new Tick { InstrumentID = "pp2205", TradingDay = baseDay, TimeStamp = baseTime.AddSeconds(count * 6 + 5), LastPrice = 5555, Volume = 555, OpenInterest = 55, PreClosePrice = 5555, PreSettlementPrice = 5555, PreOpenInterest = 55, AskPrice = 5555, AskVolume = 555, BidPrice = 5554, BidVolume = 554 });
            }

            /*
             * 2. Create mocked OHLC.
             */
            for (var count = 0; count < 2; ++count)
            {
                MockedOHLCs.Add(new OHLC { InstrumentID = "l2205", TradingDay = baseDay, TimeStamp = baseTime.AddMinutes(count * 6), OpenPrice = 5555, HighPrice = 5555, LowPrice = 5555, ClosePrice = 5555, OpenInterest = 55, Volume = 555, Time = TimeSpan.FromMinutes(1) });
                MockedOHLCs.Add(new OHLC { InstrumentID = "l2205", TradingDay = baseDay, TimeStamp = baseTime.AddMinutes(count * 6 + 1), OpenPrice = 5555, HighPrice = 5555, LowPrice = 5555, ClosePrice = 5555, OpenInterest = 55, Volume = 555, Time = TimeSpan.FromMinutes(1) });
                MockedOHLCs.Add(new OHLC { InstrumentID = "pp2205", TradingDay = baseDay, TimeStamp = baseTime.AddMinutes(count * 6 + 2), OpenPrice = 5555, HighPrice = 5555, LowPrice = 5555, ClosePrice = 5555, OpenInterest = 55, Volume = 555, Time = TimeSpan.FromMinutes(1) });
                MockedOHLCs.Add(new OHLC { InstrumentID = "l2205", TradingDay = baseDay, TimeStamp = baseTime.AddMinutes(count * 6 + 3), OpenPrice = 5555, HighPrice = 5555, LowPrice = 5555, ClosePrice = 5555, OpenInterest = 55, Volume = 555, Time = TimeSpan.FromMinutes(1) });
                MockedOHLCs.Add(new OHLC { InstrumentID = "pp2205", TradingDay = baseDay, TimeStamp = baseTime.AddMinutes(count * 6 + 4), OpenPrice = 5555, HighPrice = 5555, LowPrice = 5555, ClosePrice = 5555, OpenInterest = 55, Volume = 555, Time = TimeSpan.FromMinutes(1) });
                MockedOHLCs.Add(new OHLC { InstrumentID = "pp2205", TradingDay = baseDay, TimeStamp = baseTime.AddMinutes(count * 6 + 5), OpenPrice = 5555, HighPrice = 5555, LowPrice = 5555, ClosePrice = 5555, OpenInterest = 55, Volume = 555, Time = TimeSpan.FromMinutes(1) });
            }

            /*
             * 3. Create mocked instrument updates.
             */
            MockedInstruments.Add(new Instrument { InstrumentID = "l2205", TradingDay = baseDay, Status = InstrumentStatus.AuctionOrdering, EnterTime = baseTime.AddHours(1) });
            MockedInstruments.Add(new Instrument { InstrumentID = "l2205", TradingDay = baseDay, Status = InstrumentStatus.AuctionBalance, EnterTime = baseTime.AddHours(2) });
            MockedInstruments.Add(new Instrument { InstrumentID = "l2205", TradingDay = baseDay, Status = InstrumentStatus.AuctionMatch, EnterTime = baseTime.AddHours(3) });
            MockedInstruments.Add(new Instrument { InstrumentID = "l2205", TradingDay = baseDay, Status = InstrumentStatus.BeforeTrading, EnterTime = baseTime.AddHours(4) });
            MockedInstruments.Add(new Instrument { InstrumentID = "l2205", TradingDay = baseDay, Status = InstrumentStatus.Continous, EnterTime = baseTime.AddHours(5) });
            MockedInstruments.Add(new Instrument { InstrumentID = "l2205", TradingDay = baseDay, Status = InstrumentStatus.NoTrading, EnterTime = baseTime.AddHours(6) });
            MockedInstruments.Add(new Instrument { InstrumentID = "l2205", TradingDay = baseDay, Status = InstrumentStatus.Continous, EnterTime = baseTime.AddHours(7) });
            MockedInstruments.Add(new Instrument { InstrumentID = "l2205", TradingDay = baseDay, Status = InstrumentStatus.Closed, EnterTime = baseTime.AddHours(8) });

            MockedInstruments.Add(new Instrument { InstrumentID = "pp2205", TradingDay = baseDay, Status = InstrumentStatus.AuctionOrdering, EnterTime = baseTime.AddHours(1) });
            MockedInstruments.Add(new Instrument { InstrumentID = "pp2205", TradingDay = baseDay, Status = InstrumentStatus.AuctionBalance, EnterTime = baseTime.AddHours(2) });
            MockedInstruments.Add(new Instrument { InstrumentID = "pp2205", TradingDay = baseDay, Status = InstrumentStatus.AuctionMatch, EnterTime = baseTime.AddHours(3) });
            MockedInstruments.Add(new Instrument { InstrumentID = "pp2205", TradingDay = baseDay, Status = InstrumentStatus.BeforeTrading, EnterTime = baseTime.AddHours(4) });
            MockedInstruments.Add(new Instrument { InstrumentID = "pp2205", TradingDay = baseDay, Status = InstrumentStatus.Continous, EnterTime = baseTime.AddHours(5) });
            MockedInstruments.Add(new Instrument { InstrumentID = "pp2205", TradingDay = baseDay, Status = InstrumentStatus.NoTrading, EnterTime = baseTime.AddHours(6) });
            MockedInstruments.Add(new Instrument { InstrumentID = "pp2205", TradingDay = baseDay, Status = InstrumentStatus.Continous, EnterTime = baseTime.AddHours(7) });
            MockedInstruments.Add(new Instrument { InstrumentID = "pp2205", TradingDay = baseDay, Status = InstrumentStatus.Closed, EnterTime = baseTime.AddHours(8) });
        }

        protected void CompareCollection<T>(ICollection<T> c0, ICollection<T> c1)
        {
            if (c0.Count != c1.Count)
            {
                throw new ArgumentException("Arguments not the same length, " + c0.Count + " and " + c1.Count + ".");
            }

            /*
             * Find the first different element in c1 compared to the element in c0 that has the same index.
             * If all elements are the same, returns int.MaxValue.
             */
            int index = -1;
            while (++index < c1.Count)
            {
                var e0 = c0.ElementAt(index) ?? throw new NullReferenceException("Left operand has no value at index " + index + ".");

                /*
                 * Struct's Equals method compares the value.
                 */
                if (!e0.Equals(c1.ElementAt(index)))
                {
                    throw new ArgumentException("Element not the same at index " + index + ".");
                }
            }
        }
    }
}
