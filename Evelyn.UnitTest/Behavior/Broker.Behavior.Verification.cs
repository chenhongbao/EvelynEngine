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

namespace Evelyn.UnitTest.Behavior
{
    [TestClass]
    public class BrokerBehaviorVerification
    {
        internal IEvelyn Engine { get; private set; } = IEvelyn.NewInstance;
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal MockedLocalClient Client { get; private set; } = new MockedLocalClient();
        internal DateTime BaseTime { get; private set; } = DateTime.Now;
        internal DateOnly TradingDay { get; private set; } = DateOnly.FromDateTime(DateTime.Now);

        [TestInitialize]
        public void Initialize()
        {
            var BaseTime = DateTime.Now;

            TradingDay = DateOnly.FromDateTime(BaseTime);
            Engine = IEvelyn.NewInstance
                .RegisterLocalClient("MOCKED_CLIENT", Client, "l2205", "pp2205")
                .RegisterInstrument(
                 new Instrument
                 {
                     InstrumentID = "l2205",
                     TradingDay = TradingDay,
                     Status = InstrumentStatus.Continous,
                     EnterTime = BaseTime
                 },
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.Closed,
                    EnterTime = BaseTime
                })
                .Configure(Configurator);

            Configurator.Broker.MockedConnect(true);
        }

        [TestMethod("Request new order when instrument is not continous.")]
        public void OrderWhenNonContinous()
        {
            /*
             * Switch instrument state to non-continous, then request new order.
             * 
             * New order withour option is routed to broker directly by default,
             * no matter what state instrument has.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "l2205",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.Closed,
                    EnterTime = DateTime.Now
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
             * Broker receives 1 new order request.
             */
            Assert.AreEqual(1, Configurator.Broker.ReceivedNewOrders.Count);

            var order = Configurator.Broker.ReceivedNewOrders[0];

            Assert.AreEqual("l2205", order.InstrumentID);
            Assert.AreEqual(8888, order.Price);
            Assert.AreEqual(2, order.Quantity);
            Assert.AreEqual(Direction.Buy, order.Direction);
            Assert.AreEqual(Offset.Open, order.Offset);
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
                    Status = InstrumentStatus.Continous,
                    EnterTime = DateTime.Now
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
             * Broker receives 1 new order request, with the order ID that is rewritten by engine.
             */
            Assert.AreEqual(1, Configurator.Broker.ReceivedNewOrders.Count);

            var order = Configurator.Broker.ReceivedNewOrders[0];

            Assert.AreEqual("pp2205", order.InstrumentID);
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
                    OrderID = order.OrderID,
                    Price = 8555,
                    Quantity = 2,
                    Direction = Direction.Buy,
                    Offset = Offset.Open,
                    TradeID = "MOCKED_ORDER_1_TRADE_1",
                    TradePrice = 0,
                    TradeQuantity = 0,
                    LeaveQuantity = 2,
                    Status = OrderStatus.Rejected,
                    Message = "Rejected"
                },
                new Description
                {
                    Code = ErrorCodes.SimBrokerDuplicatedOrders,
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

        [TestMethod("Open order with option")]
        public void OpenOrderWithOption()
        {
            /*
             * Open order at instrument state change to Continous.
             * 
             * 1. Switch instrument state to NonTrading.
             * 2. Request new order with option set to InstrumentStateChange and state of Continous.
             * 3. Broker receives no request by now.
             * 4. Switch instrument state to Continous.
             * 5. Broker shall receive the pending request.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "l2205",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.NoTrading,
                    EnterTime = DateTime.Now
                });

            /*
             * Client requests new order and the requesting order is kept by engine.
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
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.StateChange,
                        StateChange = InstrumentStatus.Continous
                    }
                });

            /*
             * Broker receives no request by now.
             */
            Assert.AreEqual(0, Configurator.Broker.ReceivedNewOrders.Count);

            /*
             * Instrument state is changed to Continous.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "l2205",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.Continous,
                    EnterTime = DateTime.Now
                });

            /*
             * Broker shall receive the request, the order ID is rewriten by engine.
             */
            Assert.AreEqual(1, Configurator.Broker.ReceivedNewOrders.Count);

            var order = Configurator.Broker.ReceivedNewOrders[0];

            Assert.AreEqual("l2205", order.InstrumentID);
            Assert.AreEqual(8888, order.Price);
            Assert.AreEqual(2, order.Quantity);
            Assert.AreEqual(Direction.Buy, order.Direction);
            Assert.AreEqual(Offset.Open, order.Offset);

        }

        [TestMethod("Delete order with option")]
        public void DeleteOrderWithOption()
        {
            /*
             * Delete an existing order with state change option.
             * 
             * 1. Instrument state is set to Continous.
             * 2. Client request new order.
             * 3. Instrument state is set to NonTrading.
             * 4. Client deletes the previous order with option of state change.
             * 5. Instrument state is set to Continous.
             * 6. Broker receives the pending deletion request.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.Continous,
                    EnterTime = DateTime.Now
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
             * Instrument state is changed to NonTrading.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.NoTrading,
                    EnterTime = DateTime.Now
                });

            /*
             * Client deletes the order with option.
             */
            Client.MockedDelete(
                new DeleteOrder
                {
                    OrderID = "MOCKED_ORDER_1",
                    InstrumentID = "pp2205"
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.StateChange,
                        StateChange = InstrumentStatus.Continous
                    }
                });

            /*
             * Broker doesn't receive the request because instrument state is not changed.
             */
            Assert.AreEqual(0, Configurator.Broker.ReceivedDeleteOrders.Count);

            /*
             * Instrument state is set to Continous.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "pp2205",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.Continous,
                    EnterTime = DateTime.Now
                });

            /*
             * Broker receives the request.
             */
            Assert.AreEqual(1, Configurator.Broker.ReceivedDeleteOrders.Count);
        }

        [TestMethod("Order with option that triggers immediately.")]
        public void OrderWithOptionTriggerNow()
        {
            /*
             * Order with option that trigger right now is routed to broker, no waiting.
             * 
             * 1. Order with instrument state option, triggers right away.
             * 2. Order with time option, triggers right away.
             */

            /*
             * Instrument is at continous state.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "l2205",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.Continous,
                    EnterTime = DateTime.Now
                });

            /*
             * 1. Client requests new order with option that instrument is in continous state.
             * 
             *    And this order is sent immediately.
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
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.StateChange,
                        StateChange = InstrumentStatus.Continous
                    }
                });

            /*
             * Check broker receives the new request.
             */
            Assert.AreEqual(1, Configurator.Broker.ReceivedNewOrders.Count);

            var order = Configurator.Broker.ReceivedNewOrders[0];

            Assert.AreEqual("l2205", order.InstrumentID);
            Assert.AreEqual(8888, order.Price);
            Assert.AreEqual(2, order.Quantity);
            Assert.AreEqual(Direction.Buy, order.Direction);
            Assert.AreEqual(Offset.Open, order.Offset);

            /*
             * 2. Request a new order with time option that elapses right away.
             * 
             *    The order is sent immediately.
             */
            Client.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_2",
                    Price = 7777,
                    Quantity = 3,
                    Direction = Direction.Sell,
                    Offset = Offset.Open,
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.Time,
                        Time = BaseTime.AddSeconds(-1)
                    }
                });

            /*
             * Since engine uses tick's time stamp as current time, it needs to feed
             * some ticks to trigger the order.
             */
            Configurator.FeedSource.MockedReceive(new Tick
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                TimeStamp = BaseTime
            });

            /*
             * Check broker receives the second request.
             */
            Assert.AreEqual(2, Configurator.Broker.ReceivedNewOrders.Count);

            var secondOrder = Configurator.Broker.ReceivedNewOrders[1];

            Assert.AreEqual("l2205", secondOrder.InstrumentID);
            Assert.AreEqual(7777, secondOrder.Price);
            Assert.AreEqual(3, secondOrder.Quantity);
            Assert.AreEqual(Direction.Sell, secondOrder.Direction);
            Assert.AreEqual(Offset.Open, secondOrder.Offset);
        }

        [TestMethod("New order with duplicated order ID.")]
        public void NewOrderWithDuplicatedID()
        {
            /*
             * Engine checks order ID before routing it to broker. If there is already the same ID,
             * returns error response.
             * 
             * 1. Client requests a new order with a distinct ID, broker receives the order.
             * 2. Client requests another order with the same ID.
             * 3. Engine blocks the request and sends back error response.
             * 4. Client receives the error response, and broker receives no new request.
             */

            /*
             * 1. Client requests new order, broker receives the request.
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
             * Broker receives the order.
             */
            Assert.AreEqual(1, Configurator.Broker.ReceivedNewOrders.Count);

            var order = Configurator.Broker.ReceivedNewOrders[0];

            Assert.AreEqual("pp2205", order.InstrumentID);
            Assert.AreEqual(8555, order.Price);
            Assert.AreEqual(2, order.Quantity);
            Assert.AreEqual(Direction.Buy, order.Direction);
            Assert.AreEqual(Offset.Open, order.Offset);

            /*
             * 2. Client requests another order with the same ID.
             */
            Client.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2205",
                    TradingDay = DateOnly.MaxValue,
                    TimeStamp = DateTime.MaxValue,
                    OrderID = "MOCKED_ORDER_1",
                    Price = 8900,
                    Quantity = 2,
                    Direction = Direction.Sell,
                    Offset = Offset.Close,
                });

            /*
             * 3. Engine blocks the order, and broker receives no new request.
             */
            Assert.AreEqual(1, Configurator.Broker.ReceivedNewOrders.Count);

            /*
             * Engine sends back an error response.
             */
            Assert.AreEqual(1, Client.ReceivedTrades.Count);

            var trade = Client.ReceivedTrades[0].Item1;

            Assert.AreEqual("l2205", trade.InstrumentID);
            Assert.AreEqual("MOCKED_ORDER_1", trade.OrderID);
            Assert.AreEqual(8900, trade.Price);
            Assert.AreEqual(2, trade.Quantity);
            Assert.AreEqual(Direction.Sell, trade.Direction);
            Assert.AreEqual(Offset.Close, trade.Offset);
            Assert.AreEqual(double.MaxValue, trade.TradePrice);
            Assert.AreEqual(int.MaxValue, trade.TradeQuantity);
            Assert.AreEqual(string.Empty, trade.TradeID);
            Assert.AreEqual(OrderStatus.Rejected, trade.Status);

            /*
             * Description has error information.
             */
            var description = Client.ReceivedTrades[0].Item2;

            Assert.AreEqual(ErrorCodes.DuplicatedOrder, description.Code);
        }

        [TestMethod("Delete order with an unexisting ID.")]
        public void DeleteOrderWithUnexistingID()
        {
            /*
             * Engine checks the existence of a deleting orde ID. If no such order ID found,
             * block the deletion request and sends back the error response.
             */

            Client.MockedDelete(new DeleteOrder
            {
                InstrumentID = "ANY_INSTRUMENT",
                OrderID = "NOT_EXIST_ID"
            });

            /*
             * Broker receives no deletion request.
             */
            Assert.AreEqual(0, Configurator.Broker.ReceivedNewOrders.Count);

            /*
             * Engine sends back error response.
             */
            Assert.AreEqual(1, Client.ReceivedTrades.Count);

            var trade = Client.ReceivedTrades[0].Item1;

            Assert.AreEqual("ANY_INSTRUMENT", trade.InstrumentID);
            Assert.AreEqual("NOT_EXIST_ID", trade.OrderID);
            Assert.AreEqual(OrderStatus.None, trade.Status);

            /*
             * Description has error information.
             */
            var description = Client.ReceivedTrades[0].Item2;

            Assert.AreEqual(ErrorCodes.NoSuchOrder, description.Code);
        }
    }
}
