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
using Evelyn.Model.CLI;
using Evelyn.Plugin;
using System.Text;
using System.Text.Json;

namespace Evelyn.Extension.CLI
{
    internal class ManagementConsole
    {
        private readonly IManagement _manage;

        internal ManagementConsole(IManagement manage)
        {
            _manage = manage;
        }

        internal Command? ReadCommand()
        {
            Console.Write(">>");
            return ParseCommand(Console.ReadLine());
        }

        private Command? ParseCommand(string? line)
        {
            if (line == null)
            {
                return new Command { };
            }

            string? method = null;
            var arguments = new List<string>();
            int index = 0;

            while (index < line.Length)
            {
                if (line.ElementAt(index) == '\u0020')
                {
                    ++index;
                    continue;
                }

                if (method == null)
                {
                    index = ParseMethod(line, index, out method);
                }
                else
                {
                    index = ParseArgument(line, index, out string argument);
                    arguments.Add(argument);
                }
            }

            return new Command
            {
                Method = method ?? String.Empty,
                Arguments = arguments.ToArray()
            };
        }

        private int ParseArgument(string line, int index, out string argument)
        {
            var quote = false;
            var buffer = new StringBuilder();

            if (line[index] == '"')
            {
                quote = true;
                ++index;
            }

            while(index < line.Length)
            {
                if (quote)
                {
                    if (line[index] == '"')
                    {
                        ++index;
                        break;
                    }
                    else
                    {
                        buffer.Append(line[index]);
                    }
                }
                else
                {
                    if (line[index] == '\u0020')
                    {
                        break;
                    }
                    else
                    {
                        buffer.Append(line[index]);
                    }
                }
            }

            argument = buffer.ToString();
            return index;
        }

        private int ParseMethod(string line, int index, out string? method)
        {
            var buffer = new StringBuilder();

            while(index < line.Length && line[index] != '\u0020')
            {
                buffer.Append(line[index]);
            }

            method = buffer.ToString();
            return index;
        }

        internal void WriteResult(ManagementResult<object> result)
        {
            if (result.Description.Code != 0)
            {
                Console.WriteLine("\u0020\u0020Code:\t{0}\n\u0020\u0020Message:\t{1}", result.Description.Code, result.Description.Message);
            }
            else
            {
                Console.WriteLine(JsonSerializer.Serialize(result.Result, EvelynJsonSerialization.Options).Replace("\n", "\n\u0020\u0020"));
            }
        }
    }
}