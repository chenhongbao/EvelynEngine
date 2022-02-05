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
namespace PetriSoft.Evelyn.Plugin
{
    /// <summary>
    /// Trading algorithm plugin interface.
    /// </summary>
    public interface IAlgorithm : IFeedHandler, IOrderHandler
    {
        /// <summary>
        /// Callback upon loading of the algorithm.
        /// </summary>
        /// <param name="op">Trade operator.</param>
        public void OnLoad(IOperator op);

        /// <summary>
        /// Callback upon unloading of the algorithm.
        /// </summary>
        public void OnUnload();

        public sealed new void OnUnsubscribed(string _0, Description _1)
        {
            throw new InvalidOperationException("The method is not supposed to be called.");
        }
    }
}
