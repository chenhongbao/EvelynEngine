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
namespace PetriSoft.Evelyn.Model
{
    public struct TickSpread
    {
        private double? _askPrice = null;
        private double? _bidPrice = null;
        private long? _askVolume = null;
        private long? _bidVolume = null;

        public TickSpread()
        {
        }

        public double AskPrice
        {
            get => _askPrice ?? throw new NullValueException("Ask price is null.");
            set => _askPrice = value;
        }

        public double BidPrice
        {
            get => _bidPrice ?? throw new NullValueException("Bid price is null.");
            set => _bidPrice = value;
        }

        public long AskVolume
        {
            get => _askVolume ?? throw new NullValueException("Ask volume is null.");
            set => _askVolume = value;
        }

        public long BidVolume
        {
            get => _bidVolume ?? throw new NullValueException("Bid volume is null.");
            set => _bidVolume = value;
        }
    }
}
