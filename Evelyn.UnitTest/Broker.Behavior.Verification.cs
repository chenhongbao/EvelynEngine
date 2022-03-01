﻿/*
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

namespace Evelyn.UnitTest.Behavior
{
    [TestClass]
    public class BrokerBehaviorVerification
    {
        internal IEvelyn Engine { get; private set; } = IEvelyn.New();
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal MockedLocalClient Client { get; private set; } = new MockedLocalClient();
        internal DateOnly TradingDay { get; private set; } = DateOnly.MaxValue;

        [TestInitialize]
        public void Initialize()
        {
            var baseTime = DateTime.Now;

            Engine = IEvelyn.New();
            TradingDay = DateOnly.FromDateTime(baseTime);

            Engine.EnableLocalClient("MOCKED_CLIENT", Client, "l2205")
                .InitializeInstrument(
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
                     State = InstrumentState.Continous,
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
                .Configure(Configurator);
        }

        [TestMethod("Request new order when instrument is not continous.")]
        public void OrderWhenNonContinous()
        {
            /*
             * Switch instrument state to non-continous, then request new order.
             * 
             * Engine changes internal instrument state to 'closed', and following
             * request of the instrument is blocked by engine.
             * 
             * And that means broker receives on new order request when engine's
             * instrument internal state is closed.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "l2205",
                    TradingDay = TradingDay,
                    TimeStamp = DateTime.Now,
                    Margin = 0.11,
                    Commission = 1.01,
                    Multiple = 5,
                    MarginMethod = CalculationMethod.PerAmount,
                    CommissionMethod = CalculationMethod.PerVolume,
                    State = InstrumentState.Closed,
                    StateTimestamp = DateTime.Now
                });

            /*
             * Client requests new order.
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
             * Broker receives no new order request.
             */
            Assert.AreEqual(0, Configurator.Broker.ReceivedNewOrders.Count);

            /*
             * Client receives rejected trade.
             */
            Assert.AreEqual(1, Client.ReceivedTrades.Count);

            var trade = Client.ReceivedTrades[0].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", trade.OrderID);
            Assert.AreEqual(2, trade.Quantity);
            Assert.AreEqual(0, trade.TradePrice);
            Assert.AreEqual(0, trade.TradeQuantity);
            Assert.AreEqual(2, trade.LeaveQuantity);
            Assert.AreEqual(OrderStatus.Rejected, trade.Status);
        }

        [TestMethod("Request new order when broker is disconnected.")]
        public void OrderWhenDisconnected()
        {
            /*
             * Switch instrument state to continous, then request new order.
             * 
             * Engine changes internal instrument state to 'continous', and following
             * request of the instrument is routed to broker.
             * 
             * And that means broker receives 1 new order request, but it is disconnected
             * from remote front, so broker rejects the new order.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = TradingDay,
                    TimeStamp = DateTime.Now,
                    Margin = 0.11,
                    Commission = 1.01,
                    Multiple = 5,
                    MarginMethod = CalculationMethod.PerAmount,
                    CommissionMethod = CalculationMethod.PerVolume,
                    State = InstrumentState.Continous,
                    StateTimestamp = DateTime.Now
                });

            /*
             * Client requests new order.
             */
            Client.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "pp2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8555,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                });

            /*
             * Broker receives 1 new order request.
             */
            Assert.AreEqual(1, Configurator.Broker.ReceivedNewOrders.Count);

            var order = Configurator.Broker.ReceivedNewOrders[0];

            Assert.AreEqual("pp2205", order.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", order.OrderID);
            Assert.AreEqual(8555, order.Price);
            Assert.AreEqual(2, order.Quantity);
            Assert.AreEqual(Direction.Buy, order.Direction);
            Assert.AreEqual(Offset.Open, order.Offset);

            /*
             * Broker sends rejected trade.
             */
            Configurator.Broker.MockedTrade(
                new Trade
                {
                    InstrumentID = "pp2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8555,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 0,
                    TradeQuantity = 0,
                    LeaveQuantity = 2,
                    TradeTimeStamp = DateTime.MaxValue,
                    Status = OrderStatus.Rejected,
                    Message = "Rejected"
                },
                new Description
                {
                    Code = 0,
                    Message = "Order is rejected."
                });

            /*
             * Client receives rejected trade.
             */
            Assert.AreEqual(1, Client.ReceivedTrades.Count);

            var trade = Client.ReceivedTrades[0].Item1;

            Assert.AreEqual("pp2205", trade.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", trade.OrderID);
            Assert.AreEqual("MOCKED_ORDER_1_TRADE_1", trade.TradeID);
            Assert.AreEqual(2, trade.Quantity);
            Assert.AreEqual(0, trade.TradePrice);
            Assert.AreEqual(0, trade.TradeQuantity);
            Assert.AreEqual(2, trade.LeaveQuantity);
            Assert.AreEqual(OrderStatus.Rejected, trade.Status);
        }
    }
}
