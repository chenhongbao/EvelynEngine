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
using Evelyn.Plugin;

namespace Evelyn.Extension.CLI
{
    public static class ManagementExtensions
    {
        private static readonly Dictionary<string, List<string>> _clientSubscription = new Dictionary<string, List<string>>();

        public static ManagementResult<bool> PauseClient(this IManagement management, string clientID)
        {
            var client = management.QueryClients().Result.Clients.Find(client => client.ClientID == clientID);
            if (client.Equals(default(ClientBrief)))
            {
                return new ManagementResult<bool>
                {
                    Result = false,
                    Description = new Description { Code = 32, Message = "No such client with ID " + clientID }
                };
            }
            else
            {
                _clientSubscription.Add(clientID, new List<string>(client.Subscription));

                var r = management.AlterClient(clientID);
                return new ManagementResult<bool>
                {
                    Result = (r.Description.Code == 0),
                    Description = r.Description
                };
            }
        }

        public static ManagementResult<bool> ResumeClient(this IManagement management, string clientID)
        {
            if (_clientSubscription.ContainsKey(clientID))
            {
                var r = management.AlterClient(clientID, _clientSubscription[clientID].ToArray());
                _clientSubscription.Remove(clientID);

                return new ManagementResult<bool>
                {
                    Result = r.Description.Code == 0,
                    Description = r.Description
                };
            }
            else
            {
                return new ManagementResult<bool>
                {
                    Result = false,
                    Description = new Description { Code = 32, Message = "No pausing client with ID " + clientID }
                };
            }
        }
    }
}
