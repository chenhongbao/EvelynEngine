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
    /// Local algorithm client.
    /// </summary>
    public class LocalClient
    {
        private readonly IAlgorithm _algorithm;
        private readonly ClientAccount _account;
        private readonly ClientPosition _position;
        private readonly List<Instrument> _instruments = new List<Instrument>();

        /// <summary>
        /// Construct a local algorithm client with the specified initial account and position.
        /// </summary>
        /// <param name="algorithm">Algorithm to run in the local client.</param>
        /// <param name="account">Initial client's account.</param>
        /// <param name="position">Initial client's position.</param>
        /// <param name="instruments">Instrument information for this client.</param>
        public LocalClient(IAlgorithm algorithm, ClientAccount account, ClientPosition position, params Instrument[] instruments)
        {
            _algorithm = algorithm;
            _account = account;
            _position = position;
            _instruments.AddRange(instruments);
        }

        /// <summary>
        /// Client's algorithm.
        /// </summary>
        public IAlgorithm Algorithm => _algorithm;

        /// <summary>
        /// Client's account.
        /// </summary>
        public ClientAccount Account => _account;

        /// <summary>
        /// Client's position.
        /// </summary>
        public ClientPosition Position => _position;

        /// <summary>
        /// Client's subscribing instruments.
        /// <para>The instrument information is also used for account management.</para>
        /// </summary>
        public IReadOnlyCollection<Instrument> Instruments => _instruments;
    }
}
