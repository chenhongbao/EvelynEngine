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
    /// Configurator for algorithm and underlying broker and data feed source.
    /// </summary>
    public interface IConfigurator
    {
        /// <summary>
        /// Create customized broker and data feed source.
        /// </summary>
        /// <param name="broker">Broker output parameter.</param>
        /// <param name="feedSource">Data feed source output parameter.</param>
        public void Create(out IBroker broker, out IFeedSource feedSource);

        /// <summary>
        /// Start the broker and data feed source.
        /// </summary>
        public void Start();

        /// <summary>
        /// Stop the broker and data feed source.
        /// </summary>
        public void Stop();

        /// <summary>
        /// Destroy and resources for broker and data feed source.
        /// </summary>
        public void Desrtoy();
    }
}
