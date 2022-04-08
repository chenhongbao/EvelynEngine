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

namespace Evelyn.Extension.UnitTest
{
    public class SimulatedConfiguratorData
    {
        internal List<Tick> Ticks { get; init; } = new List<Tick>();
        internal List<OHLC> OHLCs { get; init; } = new List<OHLC>();
        internal List<Instrument> Instruments { get; init; } = new List<Instrument>();
        internal MockedExchange BrokerExchange { get; private set; } = new MockedExchange();
        internal MockedExchange FeedSourceExchange { get; private set; } = new MockedExchange();
        internal MockedOrderHandler OrderHandler { get; private set; } = new MockedOrderHandler();
        internal MockedFeedHandler FeedHandler { get; private set; } = new MockedFeedHandler();
        internal DateTime BaseTime { get; init; } = DateTime.Now;
        internal DateOnly TradingDay { get; init; } = DateOnly.FromDateTime(DateTime.Now);

        public void Initialize()
        {
            /*
             * Reset data.
             */
            BrokerExchange = new MockedExchange();
            FeedSourceExchange = new MockedExchange();
            OrderHandler = new MockedOrderHandler();
            FeedHandler = new MockedFeedHandler();

            Instruments.Clear();
            Ticks.Clear();

            /*
             * Two instruments.
             */
            Instruments.Add(new Instrument { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, Status = InstrumentStatus.Continous, EnterTime = BaseTime.AddSeconds(1) });
            Instruments.Add(new Instrument { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, Status = InstrumentStatus.Continous, EnterTime = BaseTime.AddSeconds(2) });
            Instruments.Add(new Instrument { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, Status = InstrumentStatus.Closed, EnterTime = BaseTime.AddHours(31) });
            Instruments.Add(new Instrument { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, Status = InstrumentStatus.Closed, EnterTime = BaseTime.AddHours(32) });

            /*
             * Ticks.
             */
            Ticks.Add(new Tick { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(11), LastPrice = 8900, Volume = 100, OpenInterest = 1000, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8900, AskVolume = 10, BidPrice = 8898, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(12), LastPrice = 8899, Volume = 110, OpenInterest = 1010, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8899, AskVolume = 10, BidPrice = 8898, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(13), LastPrice = 8897, Volume = 120, OpenInterest = 1020, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8898, AskVolume = 10, BidPrice = 8897, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(14), LastPrice = 8898, Volume = 130, OpenInterest = 1030, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8898, AskVolume = 10, BidPrice = 8896, BidVolume = 10 });

            Ticks.Add(new Tick { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(21), LastPrice = 8500, Volume = 100, OpenInterest = 1000, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8500, AskVolume = 10, BidPrice = 8498, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(22), LastPrice = 8499, Volume = 110, OpenInterest = 1010, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8499, AskVolume = 10, BidPrice = 8498, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(23), LastPrice = 8497, Volume = 120, OpenInterest = 1020, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8498, AskVolume = 10, BidPrice = 8497, BidVolume = 10 });
            Ticks.Add(new Tick { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddSeconds(24), LastPrice = 8498, Volume = 130, OpenInterest = 1030, PreClosePrice = 8800, PreSettlementPrice = 8800, PreOpenInterest = 990, AskPrice = 8498, AskVolume = 10, BidPrice = 8496, BidVolume = 10 });

            /*
             * OHLC
             */
            OHLCs.Add(new OHLC { InstrumentID = "l2205", ExchangeID = "DCE", Symbol = "塑料2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddMinutes(1), OpenPrice = 8900, HighPrice = 8900, LowPrice = 8897, ClosePrice = 8898, OpenInterest = 1030, Volume = 30, Time = TimeSpan.FromMinutes(1) });
            OHLCs.Add(new OHLC { InstrumentID = "pp2205", ExchangeID = "DCE", Symbol = "聚丙烯2205", TradingDay = TradingDay, TimeStamp = BaseTime.AddMinutes(1), OpenPrice = 8500, HighPrice = 8500, LowPrice = 8497, ClosePrice = 8498, OpenInterest = 1030, Volume = 30, Time = TimeSpan.FromMinutes(1) });
        }
    }
}
