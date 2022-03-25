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
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Evelyn.Extension.CLI
{
    internal class Command
    {
        internal string Method { get; set; } = String.Empty;
        internal string[] Arguments { get; set; } = new string[0];

        internal ManagementResult<object> Invoke(IManagement manage)
        {
            var method = manage.GetType().GetMethod(Method);
            if (method == null)
            {
                return new ManagementResult<object>
                {
                    Result = new Object(),
                    Description = new Description
                    {
                        Code = 28,
                        Message = "No method \'" + Method + "\'"
                    }
                };
            }

            var paramInfo = method.GetParameters();
            if (paramInfo.Length != Arguments.Length)
            {
                return new ManagementResult<object>
                {
                    Result = new Object(),
                    Description = new Description
                    {
                        Code = 29,
                        Message = "Invalid parameters, require (" + ParameterString(paramInfo) + ")"
                    }
                };
            }

            List<object> args = new List<object>();
            for (int index = 0; index < Arguments.Length; ++index)
            {
                if (paramInfo[index].ParameterType == typeof(DateTime))
                {
                    var param = ParseAllDateTimes(Arguments[index]);
                    if (param != null)
                    {
                        args.Add(param);
                    }
                    else
                    {
                        return new ManagementResult<object>
                        {
                            Result = new Object(),
                            Description = new Description
                            {
                                Code = 30,
                                Message = "Can't parse parameter \'" + Arguments[index] + "\' as " + paramInfo[index].ParameterType.Name
                            }
                        };
                    }
                }
                else if (paramInfo[index].ParameterType == typeof(LogLevel))
                {
                    var param = ParseLogLevel(Arguments[index]);
                    if (param != null)
                    {
                        args.Add(param);
                    }
                    else
                    {
                        return new ManagementResult<object>
                        {
                            Result = new Object(),
                            Description = new Description
                            {
                                Code = 30,
                                Message = "Can't parse parameter \'" + Arguments[index] + "\' as " + paramInfo[index].ParameterType.Name
                            }
                        };
                    }
                }
                else if (paramInfo[index].ParameterType == typeof(string[]))
                {
                    args.Add(Arguments[index]);
                }
                else
                {
                    return new ManagementResult<object>
                    {
                        Result = new Object(),
                        Description = new Description
                        {
                            Code = 31,
                            Message = "Unsupported parameter type " + paramInfo[index].ParameterType.Name
                        }
                    };
                }
            }

            var result = method.Invoke(manage, args.ToArray());
            if (result != null)
            {
                return (ManagementResult<object>)result;
            }
            else
            {
                return new ManagementResult<object>
                {
                    Result = new object(),
                    Description = new Description
                    {
                        Code = 32,
                        Message = "Method returns null"
                    }
                };
            }
        }

        private LogLevel? ParseLogLevel(string value)
        {
            switch (value)
            {
                case "Trace": return LogLevel.Trace;
                case "Debug": return LogLevel.Debug;
                case "Information": return LogLevel.Information;
                case "Warning": return LogLevel.Warning;
                case "Error": return LogLevel.Error;
                case "Critical": return LogLevel.Critical;
                case "None": return LogLevel.None;
                default: return null;
            }
        }

        private DateTime? ParseAllDateTimes(string datetime)
        {
            if (datetime.Length == 19)
            {
                return DateTime.ParseExact(datetime, "yyyy-MM-dd HH:mm:ss", null);
            }
            else if (datetime.Length == 16)
            {
                return DateTime.ParseExact(datetime, "yyyy-MM-dd HH:mm", null);
            }
            else if (datetime.Length == 13)
            {
                return DateTime.ParseExact(datetime, "yyyy-MM-dd HH", null);
            }
            else if (datetime.Length == 10)
            {
                return DateTime.ParseExact(datetime, "yyyy-MM-dd", null);
            }
            else
            {
                return null;
            }
        }

        private string ParameterString(ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
            {
                return string.Empty;
            }
            else if (parameters.Length == 1)
            {
                return parameters[0].ParameterType.Name;
            }
            else
            {
                var str = parameters[0].ParameterType.Name;
                for (int index = 1; index < parameters.Length; ++index)
                {
                    str += ", " + parameters[index].ParameterType.Name;
                }

                return str;
            }
        }
    }
}
