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
namespace Evelyn.Model
{
    public struct ErrorCodeSetting
    {
        public int OK { get; private set; } = 0;
        public int ExchangeDisconnected { get; set; } = 1;
        public int NoSuchOrder { get; set; } = 2;
        public int DuplicatedOrder { get; set; } = 3;
        public int DuplicatedSubscription { get; set; } = 4;
        public int NoSuchSubscription { get; set; } = 5;
        public int AlterClientThrowsException { get; set; } = 6;
        public int ExitSystemThrowsException { get; set; } = 7;
        public int NoSuchClient { get; set; } = 8;
        public int QueryEngineThrowsException { get; set; } = 9;
        public int NoLocalClient { get; set; } = 10;
        public int CommandThrowsException { get; set; } = 11;
        public int NeedMoreParameters { get; set; } = 12;
        public int NeedExtactParameters { get; set; } = 13;
        public int ParsingFormatError { get; set; } = 14;
        public int UnsupportedFunction { get; set; } = 15;
        public int SimBrokerNoSuchOrder { get; set; } = 16;
        public int SimBrokerDuplicatedOrders { get; set; } = 17;
        public int SimBrokerDeletionFail { get; set; } = 18;
        public int NoSuchInstrument { get; set; } = 19;

        public ErrorCodeSetting()
        {
        }
    }
}