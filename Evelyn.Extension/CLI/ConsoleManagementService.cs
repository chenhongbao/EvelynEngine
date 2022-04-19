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
using Evelyn.Internal;
using Evelyn.Model;
using Evelyn.Plugin;

namespace Evelyn.Extension.CLI
{
    public class ConsoleManagementService : IManagementService
    {
        private readonly ManagementConsole _console = new ManagementConsole();
        private IManagement? _manage;

        public void Configure(IManagement management)
        {
            if (_manage != null)
            {
                throw new InvalidOperationException("Can't reconfigure a management service.");
            }

            _manage = management;

            ThreadPool.QueueUserWorkItem(state =>
            {
                Command? cmd;
                while ((cmd = _console.ReadCommand()) != null)
                {
                    try
                    {
                        _console.WriteResult(cmd, cmd.Invoke(_manage));
                    }
                    catch (Exception ex)
                    {
                        _console.WriteResult(cmd, new Model.CLI.ManagementResult<object>
                        {
                            Result = new object(),
                            Description = new Description
                            {
                                Code = ErrorCodes.CommandThrowsException,
                                Message = ex.Message,
                            }
                        });
                    }
                }
            });
        }
    }
}
