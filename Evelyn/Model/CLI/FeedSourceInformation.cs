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
namespace Evelyn.Model.CLI
{
    public struct FeedSourceInformation
    {
        public bool IsConnected { get; set; }
        public DateOnly TradingDay { get; set; }
        public int OHLCGeneratorCount { get; set; }
        public Dictionary<string, bool> SubscriptionResponses { get; set; }
        public List<Instrument> Instruments { get; set; }
        public List<ScheduledJobBrief> ScheduledJobs { get; set; }
    }
}