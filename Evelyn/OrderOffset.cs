/*
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
    /// <summary>
    /// Trading offsets.
    /// </summary>
    public enum OrderOffset
    {
        /// <summary>
        /// Trade to open a position.
        /// </summary>
        Open = 1,

        /// <summary>
        /// Close a position.
        /// </summary>
        Close,

        /// <summary>
        /// Close a position that is opened today.
        /// </summary>
        CloseToday,

        /// <summary>
        /// Close a position that is opened before today.
        /// </summary>
        CloseYesterday
    }
}