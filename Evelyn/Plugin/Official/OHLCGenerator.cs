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

namespace Evelyn.Plugin
{
    public class OHLCGenerator : IOHLCGenerator
    {
        private Dictionary<string, SingleOHLCGenerator> _generators = new Dictionary<string, SingleOHLCGenerator>();

        public bool Generate(Tick tick, out OHLC ohlc)
        {
            if (!_generators.TryGetValue(tick.InstrumentID, out var generator))
            {
                _generators.Add(tick.InstrumentID, generator = new SingleOHLCGenerator());
            }
            return generator.Generate(tick, out ohlc);
        }
    }

    class SingleOHLCGenerator
    {
        private long _volume;
        private DateTime _timeStamp;
        private bool _initialized = false;
        private string _instrumentID = String.Empty;
        private string _exchangeID = String.Empty;
        private string _symbol = String.Empty;
        private DateOnly _tradingDay;
        private double _openPrice;
        private double _highPrice;
        private double _lowPrice;
        private long _currentVolume;
        private double _closePrice;
        private long _openInterest;

        public bool Generate(Tick tick, out OHLC ohlc)
        {
            if (!_initialized)
            {
                ResetValues(tick);
                _volume = tick.Volume;
                _timeStamp = tick.TimeStamp;
                _initialized = true;
            }

            if (tick.TimeStamp.CompareTo(_timeStamp) < 0)
            {
                /*
                 * Replay the old market data, don't update the OHLC.
                 */
                ohlc = default(OHLC);
                return false;
            }

            if (tick.TimeStamp.Minute == _timeStamp.Minute)
            {
                _timeStamp = tick.TimeStamp;
                _closePrice = tick.LastPrice;
                _openInterest = tick.OpenInterest;
                _currentVolume = tick.Volume;

                _highPrice = _highPrice < tick.LastPrice ? tick.LastPrice : _highPrice;
                _lowPrice = _lowPrice > tick.LastPrice ? tick.LastPrice : _lowPrice;

                ohlc = default(OHLC);
                return false;
            }
            else
            {
                ohlc = new OHLC
                {
                    InstrumentID = _instrumentID,
                    ExchangeID = _exchangeID,
                    Symbol = _symbol,
                    TradingDay = _tradingDay,
                    TimeStamp = _timeStamp,
                    OpenPrice = _openPrice,
                    HighPrice = _highPrice,
                    LowPrice = _lowPrice,
                    ClosePrice = _closePrice,
                    OpenInterest = _openInterest,
                    Volume = _currentVolume - _volume,
                    Time = TimeSpan.FromMinutes(1)
                };

                ResetValues(tick);

                _volume = _currentVolume;
                _timeStamp = tick.TimeStamp;
                _openInterest = tick.OpenInterest;
                _currentVolume = tick.Volume;

                return true;
            }
        }

        private void ResetValues(Tick tick)
        {
            _instrumentID = tick.InstrumentID;
            _exchangeID = tick.ExchangeID;
            _symbol = tick.Symbol;
            _tradingDay = tick.TradingDay;
            _openPrice = _highPrice = _lowPrice = tick.LastPrice;
        }
    }
}
