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
using System.Text.Json;

namespace Evelyn.Model
{
    public class ErrorCodes
    {
        public readonly static string ConfigFile = "Evelyn.ErrorCodes.json";
        private static ErrorCodeSetting _codes = new ErrorCodeSetting();

        public static int OK => _codes.OK;
        public static int ExchangeDisconnected => _codes.ExchangeDisconnected;
        public static int NoSuchOrder => _codes.NoSuchOrder;
        public static int DuplicatedOrder => _codes.DuplicatedOrder;
        public static int DuplicatedSubscription => _codes.DuplicatedSubscription;
        public static int NoSuchSubscription => _codes.NoSuchSubscription;
        public static int AlterClientThrowsException => _codes.AlterClientThrowsException;
        public static int ExitSystemThrowsException => _codes.ExitSystemThrowsException;
        public static int NoSuchClient => _codes.NoSuchClient;
        public static int QueryEngineThrowsException => _codes.QueryEngineThrowsException;
        public static int NoLocalClient => _codes.NoLocalClient;
        public static int CommandThrowsException => _codes.CommandThrowsException;
        public static int NeedMoreParameters => _codes.NeedMoreParameters;
        public static int NeedExtactParameters => _codes.NeedExtactParameters;
        public static int ParsingFormatError => _codes.ParsingFormatError;
        public static int UnsupportedFunction => _codes.UnsupportedFunction;
        public static int SimBrokerNoSuchOrder => _codes.SimBrokerNoSuchOrder;
        public static int SimBrokerDuplicatedOrders => _codes.SimBrokerDuplicatedOrders;
        public static int SimBrokerDeletionFail => _codes.SimBrokerDeletionFail;
        public static int NoSuchInstrument => _codes.NoSuchInstrument;

        internal static void Initialize()
        {
            if (File.Exists(ConfigFile))
            {
                using (StreamReader reader = new StreamReader(ConfigFile))
                {
                    _codes = JsonSerializer.Deserialize<ErrorCodeSetting>(reader.ReadToEnd());
                }
            }
            else
            {
                _codes = new ErrorCodeSetting();
                using (StreamWriter writer = new StreamWriter(ConfigFile, append: false))
                {
                    writer.WriteLine(JsonSerializer.Serialize(_codes, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
        }
    }
}