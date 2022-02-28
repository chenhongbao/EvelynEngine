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
using Evelyn.UnitTest.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Evelyn.UnitTest
{
    [TestClass]
    public class InitialAccountPositionValidation
    {
        /*
         * Engine has the following types of trades.
         * 
         * 1. Trade all at once.
         * 2. Trade a part of the order at a time, and complete the whole order after some trades.
         * 3. Trade a part, and then deleted.
         * 4. Trade none, and then deleted.
         * 5. order is rejected.
         */

        internal IEvelyn Engine { get; private set; } = IEvelyn.New();
        internal MockedLocalClient Client { get; private set; } = new MockedLocalClient();
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal DateOnly TradingDay { get; private set; }

        [TestInitialize]
        public void Initialize()
        {
            var baseTime = DateTime.Now;
            TradingDay = DateOnly.FromDateTime(baseTime);

            Engine = IEvelyn.New();
            Client = new MockedLocalClient();
            Configurator = new MockedConfigurator();

            var position = new Position();

            position.Contracts.Add(
                new Contract
                {
                    InstrumentID = "l2205",
                    TradingDay = TradingDay,
                    TimeStamp = baseTime,
                    Direction = Direction.Buy,
                    Status = ContractStatus.Open,
                    Price = 8900
                });
            position.Contracts.Add(
                new Contract
                {
                    InstrumentID = "l2205",
                    TradingDay = TradingDay,
                    TimeStamp = baseTime,
                    Direction = Direction.Sell,
                    Status = ContractStatus.Open,
                    Price = 9000
                });
            position.Contracts.Add(
                new Contract
                {
                    InstrumentID = "pp2205",
                    TradingDay = TradingDay,
                    TimeStamp = baseTime,
                    Direction = Direction.Sell,
                    Status = ContractStatus.Open,
                    Price = 8500
                });

            Engine.RegisterInstrument(
                new Instrument
                {
                    InstrumentID = "l2205",
                    TradingDay = TradingDay,
                    TimeStamp = baseTime,
                    Margin = 0.11,
                    Commission = 1.01,
                    Multiple = 5,
                    MarginMethod = CalculationMethod.PerAmount,
                    CommissionMethod = CalculationMethod.PerVolume,
                    State = InstrumentState.Closed,
                    StateTimestamp = baseTime
                },
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = TradingDay,
                    TimeStamp = baseTime,
                    Margin = 0.11,
                    Commission = 1.01,
                    Multiple = 5,
                    MarginMethod = CalculationMethod.PerAmount,
                    CommissionMethod = CalculationMethod.PerVolume,
                    State = InstrumentState.Closed,
                    StateTimestamp = baseTime
                })
                .EnableLocalClient("InitialPositionClient", Client, new Account { Balance = 100000 }, position, "l2205")
                .Configure(Configurator);
        }

        [TestMethod("Trade all at once.")]
        public void TradeAllAtOnce()
        {

        }

        [TestMethod("Trade in parts.")]
        public void TradeInParts()
        {

        }

        [TestMethod("Trade and delete.")]
        public void TradeAndDelete()
        {

        }

        [TestMethod("Trade none and delete.")]
        public void TradeNoneAndDelete()
        {

        }

        [TestMethod("Order is rejected.")]
        public void RejectOrder()
        {

        }
    }
}
