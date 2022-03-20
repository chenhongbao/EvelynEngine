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
using System.Text.Json.Serialization;

namespace Evelyn.Extension
{
    public class EvelynJsonSerialization
    {
        public static JsonSerializerOptions Options
        {
            get
            {
                var options = new JsonSerializerOptions();

                options.WriteIndented = true;
                options.Converters.Add(new JsonTradingDayConverter());
                options.Converters.Add(new JsonTimeStampConverter());

                return options;
            }
        }

        internal class JsonTradingDayConverter : JsonConverter<DateOnly>
        {
            public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateOnly.ParseExact(reader.GetString() ?? String.Empty, "yyyyMMdd");

            public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("yyyyMMdd"));
        }

        internal class JsonTimeStampConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateTime.ParseExact(reader.GetString() ?? String.Empty, "yyyyMMdd HH:mm:ss.fffffff", null);

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("yyyyMMdd HH:mm:ss.fffffff"));
        }
    }
}
