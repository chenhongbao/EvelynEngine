﻿/*
Event of specified instrument for Evelyn Engine, a quantitative trading engine by Chen Hongbao.
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
namespace PetriSoft.Evelyn
{
    public record class InstrumentEvent : MarketItem
    {
        /// <summary>
        /// Instrument event types.
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// Before market open.
            /// </summary>
            BeforeTrading = 1,

            /// <summary>
            /// Non trading between trading sessions of a day.
            /// </summary>
            NonTrading,

            /// <summary>
            /// Auction ordering before contious trading.
            /// </summary>
            AuctionOrdering,

            /// <summary>
            /// Auction balance.
            /// </summary>
            AuctionBalance,

            /// <summary>
            /// Auction match.
            /// </summary>
            AuctionMatch,

            /// <summary>
            /// Continous trading within a trading session.
            /// </summary>
            Continous,

            /// <summary>
            /// Instrument trading is closed.
            /// </summary>
            Closed
        }

        private EventType? _event;
        private DateTime? _enterTime;

        /// <summary>
        /// Instrument event type, or throw <see cref="NullValueException"/> if the type is null.
        /// </summary>
        public EventType Event
        {
            get
            {
                return _event ?? throw new NullValueException("Event type is null.");
            }
            set
            {
                _event = value;
            }
        }

        /// <summary>
        /// Enter time of the instrument's event, or throw <see cref="NullValueException"/> if the value is null.
        /// </summary>
        public DateTime EnterTime
        {
            get
            {
                return _enterTime ?? throw new NullValueException("Enter time is null.");
            }
            set
            {
                _enterTime = value;
            }
        }
    }
}
