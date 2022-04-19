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
using System.Linq;

namespace Evelyn.UnitTest.Behavior
{
    [TestClass]
    public class ConditionalOrderVerification
    {
        private IEvelyn? Engine { get; set; }
        private MockedConfigurator? Configurator { get; set; }
        private MockedLocalClient? Client { get; set; }
        private DateTime BaseTime { get; } = DateTime.Now;

        [TestInitialize]
        public void Initialize()
        {
            Engine = IEvelyn.NewInstance
                .RegisterLocalClient("MOCKED_CLIENT", Client = new MockedLocalClient(), "l2209")
                .Configure(Configurator = new MockedConfigurator());
            Configurator.Broker.MockedConnect(true);
            Configurator.FeedSource.MockedConnect(true);
        }

        [TestMethod("Order with state change condition.")]
        public void OrderStateChange()
        {
            /*
             * Order is pending until instrument's state is changed.
             * 
             * 1. Exchange is connected, but instrument's state is not triggered.
             *    Request the order, but broker doesn't receive it.
             * 2. Instrument's state is tiggered, order is sent.
             */

            Client?.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2209",
                    ExchangeID = "DCE",
                    OrderID = "STATE_CONDITION_1",
                    Price = 9000,
                    Quantity = 1,
                    Offset = Offset.Open,
                    Direction = Direction.Buy
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
             * Now the engine has no instrument status associated with the specified instrument, so
             * it won't send the order.
             */
            Assert.AreEqual(0, Configurator?.Broker.ReceivedNewOrders.Count);

            /*
             * 1. Update the instrument state to NoTrading, and request the order.
             *    Broker doesn't receive the order.
             */
            Configurator?.FeedSource.MockedReceive(new Instrument
            {
                InstrumentID = "l2209",
                ExchangeID = "DCE",
                Status = InstrumentStatus.NoTrading,
                EnterTime = BaseTime
            });

            /*
             * Again the broker doesn't receive the order.
             */
            Assert.AreEqual(0, Configurator?.Broker.ReceivedNewOrders.Count);

            /*
             * Send another order with the same condition, broker still not receiving it.
             * Now there are two pending orders.
             */
            Client?.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2209",
                    ExchangeID = "DCE",
                    OrderID = "STATE_CONDITION_2",
                    Price = 9001,
                    Quantity = 1,
                    Offset = Offset.Open,
                    Direction = Direction.Buy
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.StateChange,
                        StateChange = InstrumentStatus.Continous
                    }
                });

            Assert.AreEqual(0, Configurator?.Broker.ReceivedNewOrders.Count);

            /*
             * 2. Send triggering instrument state.
             */
            Configurator?.FeedSource.MockedReceive(new Instrument
            {
                InstrumentID = "l2209",
                ExchangeID = "DCE",
                Status = InstrumentStatus.Continous,
                EnterTime = BaseTime
            });

            /*
             * Now the two pending orders are sent to broker.
             */
            Assert.AreEqual(2, Configurator?.Broker.ReceivedNewOrders.Count);

            Assert.AreEqual(1, Configurator?.Broker.ReceivedNewOrders.Where(order => order.Price == 9000).Count());
            Assert.AreEqual(1, Configurator?.Broker.ReceivedNewOrders.Where(order => order.Price == 9001).Count());

            /*
             * Now send the third order and broker receives it.
             */
            Client?.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2209",
                    ExchangeID = "DCE",
                    OrderID = "STATE_CONDITION_3",
                    Price = 9002,
                    Quantity = 1,
                    Offset = Offset.Open,
                    Direction = Direction.Buy
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.StateChange,
                        StateChange = InstrumentStatus.Continous
                    }
                });

            Assert.AreEqual(3, Configurator?.Broker.ReceivedNewOrders.Count);
            Assert.AreEqual(1, Configurator?.Broker.ReceivedNewOrders.Where(order => order.Price == 9002).Count());
        }

        [TestMethod("Order with time condition.")]
        public void OrderTimeCondition()
        {
            /*
             * Order is pending until time out.
             * 
             * 1. Exchange is connected, and request the order.
             * 2. Feed source mocks feeds with untriggering time stamp, broker doesn't receive the order.
             * 3. Feed source mocks the triggering feed and broker receives the order.
             * 4. Send an order with a obsolete timne condition, and broker receives it.
             */
            Client?.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2209",
                    ExchangeID = "DCE",
                    OrderID = "TIME_CONDITION_1",
                    Price = 9000,
                    Quantity = 1,
                    Offset = Offset.Open,
                    Direction = Direction.Buy
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.Time,
                        Time = BaseTime.AddMinutes(1)
                    }
                });

            /*
             * 1. There is no time information in the engine, so order is not sent.
             */
            Assert.AreEqual(0, Configurator?.Broker.ReceivedNewOrders.Count);

            /*
             * 2. Send feeds that don't trigger the time condition, and the order is not sent.
             */
            Configurator?.FeedSource.MockedReceive(new Tick
            {
                InstrumentID = "l2209",
                ExchangeID = "DCE",
                LastPrice = 9000,
                TimeStamp = BaseTime
            });

            Assert.AreEqual(0, Configurator?.Broker.ReceivedNewOrders.Count);

            /*
             * 3. Send feed with triggering time stamp and order is sent.
             */
            Configurator?.FeedSource.MockedReceive(new Tick
            {
                InstrumentID = "l2209",
                ExchangeID = "DCE",
                LastPrice = 9000,
                TimeStamp = BaseTime.AddSeconds(61)
            });

            Assert.AreEqual(1, Configurator?.Broker.ReceivedNewOrders.Count);
            Assert.AreEqual(1, Configurator?.Broker.ReceivedNewOrders.Where(order => order.Price == 9000).Count());

            /*
             * 4. Request order with time condition that triggers immediately.
             */
            Client?.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2209",
                    ExchangeID = "DCE",
                    OrderID = "TIME_CONDITION_2",
                    Price = 9001,
                    Quantity = 1,
                    Offset = Offset.Open,
                    Direction = Direction.Buy
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.Time,
                        Time = BaseTime.AddMinutes(1)
                    }
                });

            Assert.AreEqual(2, Configurator?.Broker.ReceivedNewOrders.Count);
            Assert.AreEqual(1, Configurator?.Broker.ReceivedNewOrders.Where(order => order.Price == 9001).Count());
        }

        [TestMethod("Order with combined condition.")]
        public void OrderCombinedCondition()
        {
            /*
             * Order is pending until both time and state are triggered.
             * 
             * 1. Exchange is connected, request the order but broker doesn't receive it.
             * 2. Feed source sends the triggering instrument state, broker still not receiving it.
             * 3. Feed source sends feeds that don't trigger the time condition, broker still not receiving it.
             * 4. Feed source triggers time condition, and broker receives the order.
             */
            Client?.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2209",
                    ExchangeID = "DCE",
                    OrderID = "COMBINE_CONDITION_1",
                    Price = 9000,
                    Quantity = 1,
                    Offset = Offset.Open,
                    Direction = Direction.Buy
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.Combined,
                        Time = BaseTime.AddMinutes(1),
                        StateChange = InstrumentStatus.Continous
                    }
                });

            /*
             * 1. There is no time and state information in the engine, so order is not sent.
             */
            Assert.AreEqual(0, Configurator?.Broker.ReceivedNewOrders.Count);

            /*
             * 2. Instrument state is triggered, but time condition isnt, so order is not sent.
             */
            Configurator?.FeedSource.MockedReceive(new Instrument
            {
                InstrumentID = "l2209",
                ExchangeID = "DCE",
                Status = InstrumentStatus.Continous,
                EnterTime = BaseTime
            });

            Assert.AreEqual(0, Configurator?.Broker.ReceivedNewOrders.Count);

            /*
             * 3. Send tick with untriggering time stamp, order is not sent.
             */
            Configurator?.FeedSource.MockedReceive(new Tick
            {
                InstrumentID = "l2209",
                ExchangeID = "DCE",
                LastPrice = 9000,
                TimeStamp = BaseTime
            });

            Assert.AreEqual(0, Configurator?.Broker.ReceivedNewOrders.Count);

            /*
             * 4. Trigger the time condition, and order is sent.
             */
            Configurator?.FeedSource.MockedReceive(new Tick
            {
                InstrumentID = "l2209",
                ExchangeID = "DCE",
                LastPrice = 9000,
                TimeStamp = BaseTime.AddSeconds(61)
            });

            Assert.AreEqual(1, Configurator?.Broker.ReceivedNewOrders.Count);
            Assert.AreEqual(1, Configurator?.Broker.ReceivedNewOrders.Where(order => order.Price == 9000).Count());

            /*
             * Send the order again and it is sent immediately.
             */
            Client?.MockedNewOrder(
                new NewOrder
                {
                    InstrumentID = "l2209",
                    ExchangeID = "DCE",
                    OrderID = "COMBINE_CONDITION_2",
                    Price = 9001,
                    Quantity = 1,
                    Offset = Offset.Open,
                    Direction = Direction.Buy
                },
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.Combined,
                        Time = BaseTime.AddMinutes(1),
                        StateChange = InstrumentStatus.Continous
                    }
                });

            Assert.AreEqual(2, Configurator?.Broker.ReceivedNewOrders.Count);
            Assert.AreEqual(1, Configurator?.Broker.ReceivedNewOrders.Where(order => order.Price == 9001).Count());
        }
    }
}
