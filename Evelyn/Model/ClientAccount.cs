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
    /// Settle-by-trade account.
    /// </summary>
    public class ClientAccount
    {
        private double? _balance = null;

        /// <summary>
        /// Balance containing trading profit up to date.
        /// </summary>
        public double Balance
        {
            get => _balance ?? throw new NullValueException("Balance is null.");
            set => _balance = value;
        }
    }
}