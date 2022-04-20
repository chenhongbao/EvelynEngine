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
using Evelyn.Model.CLI;
using Microsoft.Extensions.Logging;

namespace Evelyn.Plugin
{
    public interface IManagement
    {
        public ManagementResult<bool> ExitSystem();
        public ManagementResult<AlterClientResult> AlterClient(string clientID, params string[] instrumentID);

        public ManagementResult<EngineInformation> QueryEngineInformation();

        public ManagementResult<ClientsInformation> QueryClients();

        public ManagementResult<ClientOrderInformation> QueryClientOrder(string clientID, string orderID);

        public ManagementResult<ClientLogInformation> QueryClientLog(string clientID, DateTime afterTime, LogLevel logLevel = LogLevel.None);

        public ManagementResult<string> SendCommand(string clientID, params string[] commands);
    }
}
