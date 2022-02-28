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
    public class DefaultAccountPositionValidation
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
                .EnableLocalClient("DefaultAccountPositionClient", Client, "l2205")
                .Configure(Configurator);
        }

        [TestMethod("Trade all at once.")]
        public void TradeAllAtOnce()
        {
            /*
             * A default account has NaN balance, and a default position has no contract(empty).
             */
            Assert.AreEqual(double.NaN, Client.Account.Balance);
            Assert.AreEqual(0, Client.Position.Contracts.Count);

            /*
             * Trade on default account doesn't affect the account's balance, but it still
             * adds contracts to the position.
             */
            Client.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                });

            /*
             * The position now has some opening contracts(not traded yet).
             */
            Client.Position.Contracts.ForEach(contract =>
            {
                Assert.AreEqual("l2205", contract.InstrumentID);
                Assert.AreEqual(TradingDay, contract.TradingDay);
                Assert.AreEqual(8888, contract.Price);
                Assert.AreEqual(Direction.Buy, contract.Direction);
                Assert.AreEqual(ContractStatus.Opening, contract.Status);
            });

            /*
             * Trade ALL of the order.
             */
            Configurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 8890,
                    TradeQuantity = 2,
                    LeaveQuantity = 0,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Completed,
                    Message = "Completed"
                },
                new Description
                {
                    Code = 0,
                    Message = "Order is completed."
                });

            /*
             * Account's balance is still NaN, but position is changed.
             */
            Assert.AreEqual(double.NaN, Client.Account.Balance);

            Assert.AreEqual(2, Client.Position.Contracts.Count);
            Client.Position.Contracts.ForEach(contract =>
            {
                Assert.AreEqual("l2205", contract.InstrumentID);
                Assert.AreEqual(TradingDay, contract.TradingDay);
                Assert.AreEqual(8890, contract.Price);
                Assert.AreEqual(Direction.Buy, contract.Direction);
                Assert.AreEqual(ContractStatus.Open, contract.Status);
            });
        }

        [TestMethod("Trade in parts.")]
        public void TradeInParts()
        {
            /*
             * Trade on default account doesn't affect the account's balance, but it still
             * adds contracts to the position.
             */
            Client.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                });

            /*
             * Trade 1 volume of the order.
             */
            Configurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 8890,
                    TradeQuantity = 1,
                    LeaveQuantity = 1,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Trading,
                    Message = "Trading"
                },
                new Description
                {
                    Code = 0,
                    Message = "Order is trading."
                });

            /*
             * Account's balance is still NaN, but position is changed.
             * 
             * One contract is open, and the other is opening.
             */
            Assert.AreEqual(double.NaN, Client.Account.Balance);
            Assert.AreEqual(2, Client.Position.Contracts.Count);

            var c0 = Client.Position.Contracts[0];
            var c1 = Client.Position.Contracts[1];

            Assert.IsTrue((c0.Status == ContractStatus.Open && c1.Status == ContractStatus.Opening) || (c1.Status == ContractStatus.Open && c0.Status == ContractStatus.Opening));

            Client.Position.Contracts.ForEach(contract =>
            {
                switch (contract.Status)
                {
                    case ContractStatus.Opening:
                        Assert.AreEqual("l2205", contract.InstrumentID);
                        Assert.AreEqual(TradingDay, contract.TradingDay);
                        Assert.AreEqual(8888, contract.Price);
                        Assert.AreEqual(Direction.Buy, contract.Direction);
                        break;

                    case ContractStatus.Open:
                        Assert.AreEqual("l2205", contract.InstrumentID);
                        Assert.AreEqual(TradingDay, contract.TradingDay);
                        Assert.AreEqual(8890, contract.Price);
                        Assert.AreEqual(Direction.Buy, contract.Direction);
                        break;
                }
            });

            /*
             * Then trade the last 1 volume.
             */
            Configurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_2",
                    TradePrice = 8892,
                    TradeQuantity = 1,
                    LeaveQuantity = 0,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Completed,
                    Message = "Completed"
                },
                new Description
                {
                    Code = 0,
                    Message = "Order is completed."
                });

            /*
             * Account's balance is still NaN, but position has 2 open contracts with different trade prices.
             */
            Assert.AreEqual(double.NaN, Client.Account.Balance);
            Assert.AreEqual(2, Client.Position.Contracts.Count);

            Assert.IsTrue((c0.Price == 8890 && c1.Price == 8892) || (c1.Price == 8890 && c0.Price == 8892));

            Client.Position.Contracts.ForEach(contract =>
            {
                Assert.AreEqual("l2205", contract.InstrumentID);
                Assert.AreEqual(TradingDay, contract.TradingDay);
                Assert.AreEqual(ContractStatus.Open, contract.Status);
                Assert.AreEqual(Direction.Buy, contract.Direction);
            });
        }

        [TestMethod("Trade a part then delete order.")]
        public void TradeAndDelete()
        {
            /*
             * Trade on default account doesn't affect the account's balance, but it still
             * adds contracts to the position.
             */
            Client.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                });

            /*
             * Trade 1 volume of the order.
             */
            Configurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 8890,
                    TradeQuantity = 1,
                    LeaveQuantity = 1,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Trading,
                    Message = "Trading"
                },
                new Description
                {
                    Code = 0,
                    Message = "Order is trading."
                });

            /*
             * Partial trade has be verified in TradeInParts method.
             */

            /*
             * Now delete the order.
             */
            Client.MockedDelete("MOCKED_ORDER_1");

            Configurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_2",
                    TradePrice = 0, /* no actual trade happens, so price and volume are 0*/
                    TradeQuantity = 0,
                    LeaveQuantity = 1,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Deleted, /* status is deleted */
                    Message = "Deleted"
                },
                new Description
                {
                    Code = 1,
                    Message = "Order is deleted."
                });

            /*
             * Check the balance is still NaN, and position has only 1 open contract.
             */
            Assert.AreEqual(double.NaN, Client.Account.Balance);
            Assert.AreEqual(1, Client.Position.Contracts.Count);

            var contract = Client.Position.Contracts[0];

            Assert.AreEqual("l2205", contract.InstrumentID);
            Assert.AreEqual(TradingDay, contract.TradingDay);
            Assert.AreEqual(8890, contract.Price);
            Assert.AreEqual(Direction.Buy, contract.Direction);
            Assert.AreEqual(ContractStatus.Open, contract.Status);

        }

        [TestMethod("Trade none and delete.")]
        public void TradeNoneAndDelete()
        {
            Client.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                });

            /*
             * Now delete the order.
             */
            Client.MockedDelete("MOCKED_ORDER_1");

            Configurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 0, /* no actual trade happens, so price and volume are 0*/
                    TradeQuantity = 0,
                    LeaveQuantity = 2,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Deleted, /* status is deleted */
                    Message = "Deleted"
                },
                new Description
                {
                    Code = 1,
                    Message = "Order is deleted."
                });

            /*
             * Check the balance is still NaN, and position has no contract.
             */
            Assert.AreEqual(double.NaN, Client.Account.Balance);
            Assert.AreEqual(0, Client.Position.Contracts.Count);
        }

        [TestMethod("Order is rejected.")]
        public void RejectOrder()
        {
            Client.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                });

            /*
             * Now reject the order.
             */
            Configurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8888,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 0, /* no actual trade happens, so price and volume are 0*/
                    TradeQuantity = 0,
                    LeaveQuantity = 2,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Rejected, /* status is deleted */
                    Message = "Rejected"
                },
                new Description
                {
                    Code = 1,
                    Message = "Order is rejected."
                });

            /*
             * Check the balance is still NaN, and position has no contract.
             */
            Assert.AreEqual(double.NaN, Client.Account.Balance);
            Assert.AreEqual(0, Client.Position.Contracts.Count);
        }
    }
}
