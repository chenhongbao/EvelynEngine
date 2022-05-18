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

    // TODO Need refactor the code and more detailed unit test.
    class SingleOHLCGenerator
    {
        private long _volume;
        private DateTime _timeStamp;
        
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

        private bool _initialized = false;
        private bool _resetFields = false;

        public bool Generate(Tick tick, out OHLC ohlc)
        {
            if (!_initialized)
            {
                ResetValues(tick);

                _volume = tick.Volume;
                _timeStamp = tick.TimeStamp;

                _initialized = true;
                _resetFields = true;
            }

            var generated = false;

            if ((tick.TimeStamp.Hour, tick.TimeStamp.Minute) != (_timeStamp.Hour, _timeStamp.Minute))
            {
                _resetFields = false;

                generated = true;
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
            }
            else
            {
                ohlc = new OHLC();
            }

            if (tick.Volume != _currentVolume)
            {
                if (!_resetFields)
                {
                    _resetFields = true;

                    ResetValues(tick);

                    _volume = _currentVolume;
                    _openInterest = tick.OpenInterest;
                    _currentVolume = tick.Volume;
                }
                else
                {
                    _closePrice = tick.LastPrice;
                    _openInterest = tick.OpenInterest;
                    _currentVolume = tick.Volume;

                    _highPrice = _highPrice < tick.LastPrice ? tick.LastPrice : _highPrice;
                    _lowPrice = _lowPrice > tick.LastPrice ? tick.LastPrice : _lowPrice;
                }
            }

            _timeStamp = tick.TimeStamp;

            return generated;
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
