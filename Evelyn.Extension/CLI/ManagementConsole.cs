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
using Evelyn.Model.CLI;
using Evelyn.Plugin;
using System.Text;
using System.Text.Json;

namespace Evelyn.Extension.CLI
{
    internal class ManagementConsole
    {
        internal Command? ReadCommand()
        {
            Console.Write(">>");
            return ParseCommand(Console.ReadLine());
        }

        internal Command? ParseCommand(string? line)
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

                ++index;
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
                ++index;
            }

            method = buffer.ToString();
            return index;
        }

        internal void WriteResult(Command cmd, ManagementResult<object> result)
        {
            var echo = FormatEcho(result);

            /*
             * Write to console.
             */
            Console.WriteLine(echo);

            /*
             * Write console output to log file.
             */
            var file = Path.Combine(Directory.CreateDirectory(".ManagementConsole").FullName, "Query.log");
            using (StreamWriter writer = new StreamWriter(file, append: true))
            {
                writer.WriteLine(">>{0}{1}{2}", FormatCommand(cmd), Environment.NewLine, echo);
            }
        }
        
        private string FormatCommand(Command cmd)
        {
            if (cmd.Arguments.Count() == 0)
            {
                return cmd.Method;
            }
            else
            {
                return string.Format("{0}\u0020{1}", cmd.Method, cmd.Arguments.Aggregate((lhs, rhs) => string.Format("{0}\u0020{1}", lhs, rhs)));
            }
        }

        private string FormatEcho(ManagementResult<object> result)
        {
            if (result.Description.Code != ErrorCodes.OK)
            {
                return string.Format("\u0020\u0020{0}", JsonSerializer.Serialize(result.Description, EvelynJsonSerialization.Options).Replace("\n", "\n\u0020\u0020"));
            }
            else
            {
                if (result.Result is string)
                {
                    return (string)result.Result;
                }
                else if (!(result.Result is object))
                {
                    return result.Result?.ToString() ?? "{null}";
                }
                else
                {
                    return string.Format("\u0020\u0020{0}", JsonSerializer.Serialize(result.Result, EvelynJsonSerialization.Options).Replace("\n", "\n\u0020\u0020"));
                }
            }
        }
    }
}