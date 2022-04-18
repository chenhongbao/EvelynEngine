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
            switch (Method)
            {
                case "AlterClient":

                    if (Arguments.Length < 2)
                    {
                        return new ManagementResult<object>
                        {
                            Result = new object(),
                            Description = new Description
                            {
                                Code = 28,
                                Message = "Need at least 2 parameters"
                            }
                        };
                    }
                    else
                    {
                        var result = manage.AlterClient(Arguments[0], Arguments.ToList().GetRange(1, Arguments.Length - 1).ToArray());
                        return new ManagementResult<object>
                        {
                            Result = result.Result,
                            Description = result.Description
                        };
                    }

                case "QueryEngineInformation":

                    if (Arguments.Length != 0)
                    {
                        return new ManagementResult<object>
                        {
                            Result = new object(),
                            Description = new Description
                            {
                                Code = 29,
                                Message = "Need no parameter"
                            }
                        };
                    }
                    else
                    {
                        var result = manage.QueryEngineInformation();
                        return new ManagementResult<object>
                        {
                            Result = result.Result,
                            Description = result.Description
                        };

                    }

                case "QueryClients":

                    if (Arguments.Length != 0)
                    {
                        return new ManagementResult<object>
                        {
                            Result = new object(),
                            Description = new Description
                            {
                                Code = 29,
                                Message = "Need no parameter"
                            }
                        };
                    }
                    else
                    {
                        var result = manage.QueryClients();
                        return new ManagementResult<object>
                        {
                            Result = result.Result,
                            Description = result.Description
                        };

                    }

                case "QueryClientOrder":

                    if (Arguments.Length != 2)
                    {
                        return new ManagementResult<object>
                        {
                            Result = new object(),
                            Description = new Description
                            {
                                Code = 30,
                                Message = "Need exactly 2 parameters"
                            }
                        };
                    }
                    else
                    {
                        var result = manage.QueryClientOrder(Arguments[0], Arguments[1]);
                        return new ManagementResult<object>
                        {
                            Result = result.Result,
                            Description = result.Description
                        };

                    }

                case "QueryClientLog":

                    if (Arguments.Length == 2)
                    {
                        var datetime = ParseAllDateTimes(Arguments[1]);
                        if (datetime == null)
                        {
                            return new ManagementResult<object>
                            {
                                Result = new object(),
                                Description = new Description
                                {
                                    Code = 31,
                                    Message = "Can't parse date time \'" + Arguments[1] + "\'"
                                }
                            };
                        }
                        else
                        {
                            var result = manage.QueryClientLog(Arguments[0], (DateTime)datetime);
                            return new ManagementResult<object>
                            {
                                Result = result.Result,
                                Description = result.Description
                            };
                        }
                    }
                    else if (Arguments.Length == 3)
                    {
                        var datetime = ParseAllDateTimes(Arguments[1]);
                        if (datetime == null)
                        {
                            return new ManagementResult<object>
                            {
                                Result = new object(),
                                Description = new Description
                                {
                                    Code = 31,
                                    Message = "Can't parse date time \'" + Arguments[1] + "\'"
                                }
                            };
                        }
                        else
                        {
                            var logLevel = ParseLogLevel(Arguments[2]);
                            if (logLevel == null)
                            {
                                return new ManagementResult<object>
                                {
                                    Result = new object(),
                                    Description = new Description
                                    {
                                        Code = 32,
                                        Message = "Can't parse log level \'" + Arguments[2] + "\'"
                                    }
                                };
                            }
                            else
                            {
                                var result = manage.QueryClientLog(Arguments[0], (DateTime)datetime, (LogLevel)logLevel);
                                return new ManagementResult<object>
                                {
                                    Result = result.Result,
                                    Description = result.Description
                                };
                            }
                        }
                    }
                    else
                    {
                        return new ManagementResult<object>
                        {
                            Result = new object(),
                            Description = new Description
                            {
                                Code = 30,
                                Message = "Need 2 or 3 parameters"
                            }
                        };
                    }

                case "SendCommand":

                    if (Arguments.Length != 2)
                    {
                        return new ManagementResult<object>
                        {
                            Result = new object(),
                            Description = new Description
                            {
                                Code = 30,
                                Message = "Need exactly 2 parameters"
                            }
                        };
                    }
                    else
                    {
                        var result = manage.SendCommand(Arguments[0], Arguments[1]);
                        return new ManagementResult<object>
                        {
                            Result = result.Result,
                            Description = result.Description
                        };

                    }

                case "ExitSystem":
                    {
                        var timeToExit = TimeSpan.FromSeconds(1);

                        var result = manage.ExitSystem();
                        if (result.Result)
                        {
                            var timer = new System.Timers.Timer();

                            timer.Elapsed += (source, args) => Environment.Exit(0);
                            timer.Interval = timeToExit.TotalMilliseconds;
                            timer.AutoReset = false;
                            timer.Enabled = true;
                        }

                        return new ManagementResult<object>
                        {
                            Result = result.Result ? "System exits in " + timeToExit.TotalMilliseconds + "ms." : "Sytem doesn't exit.",
                            Description = result.Description
                        };
                    }

                case "PauseClient":

                    if (Arguments.Length != 1)
                    {
                        return new ManagementResult<object>
                        {
                            Result = new object(),
                            Description = new Description
                            {
                                Code = 30,
                                Message = "Need exactly 1 parameter"
                            }
                        };
                    }
                    else
                    {
                        var result = manage.PauseClient(Arguments[0]);
                        return new ManagementResult<object>
                        {
                            Result = result.Result,
                            Description = result.Description
                        };

                    }

                case "ResumeClient":

                    if (Arguments.Length != 1)
                    {
                        return new ManagementResult<object>
                        {
                            Result = new object(),
                            Description = new Description
                            {
                                Code = 30,
                                Message = "Need exactly 1 parameter"
                            }
                        };
                    }
                    else
                    {
                        var result = manage.ResumeClient(Arguments[0]);
                        return new ManagementResult<object>
                        {
                            Result = result.Result,
                            Description = result.Description
                        };

                    }

                default:
                    return new ManagementResult<object>
                    {
                        Result = new object(),
                        Description = new Description
                        {
                            Code = 28,
                            Message = "Unsupported function \'" + Method + "\'"
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
