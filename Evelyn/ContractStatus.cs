﻿/*
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
namespace PetriSoft.Evelyn
{
    /// <summary>
    /// Contract status.
    /// </summary>
    public enum ContractStatus
    {
        /// <summary>
        /// Contract is being opened and position doesn't include this contract.
        /// </summary>
        Opening = 1,

        /// <summary>
        /// Contract is open.
        /// </summary>
        Open,

        /// <summary>
        /// Contract is closing and position still include this contract.
        /// </summary>
        Closing,

        /// <summary>
        /// Contract is closed and is not included by position.
        /// </summary>
        Closed,

        /// <summary>
        /// Contract is deleted and is not included by position.
        /// </summary>
        Deleted
    }
}