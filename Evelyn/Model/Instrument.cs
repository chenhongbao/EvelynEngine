/*
Null value exception for Evelyn Engine, a quantitative trading engine by Chen Hongbao.
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
using PetriSoft.Evelyn.Exceptions;

namespace PetriSoft.Evelyn.Model
{
    /// <summary>
    /// Instrument information.
    /// </summary>
    public record class Instrument : MarketItem
    {
        private double? _margin;
        private double? _commission;
        private long? _multiple;
        private CalculationMethod? _marginMethod;
        private CalculationMethod? _commissionMethod;

        /// <summary>
        /// Margin rate.
        /// </summary>
        public double Margin
        {
            get => _margin ?? throw new NullValueException("Margin rate is null.");
            set => _margin = value;
        }

        /// <summary>
        /// Commission rate.
        /// </summary>
        public double Commission
        {
            get => _commission ?? throw new NullValueException("Commission rate is null.");
            set => _commission = value;
        }

        /// <summary>
        /// Instrument multiple.
        /// </summary>
        public long Multiple
        {
            get => _multiple ?? throw new NullValueException("Multiple is null.");
            set => _multiple = value;
        }

        /// <summary>
        /// Margin calculation method.
        /// </summary>
        public CalculationMethod MarginMethod
        {
            get => _marginMethod ?? throw new NullValueException("Margin type is null.");
            set => _marginMethod = value;
        }

        /// <summary>
        /// Commission calculation method.
        /// </summary>
        public CalculationMethod CommissionMethod
        {
            get => _commissionMethod ?? throw new NullValueException("Margin type is null.");
            set => _commissionMethod = value;
        }
    }
}