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
using Evelyn.Extension;
using Evelyn.Model;
using Evelyn.UnitTest.Behavior;
using Evelyn.UnitTest.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Evelyn.UnitTest.CLI
{
    [TestClass]
    public class FullData : DataInitialize
    {
        internal IEvelyn Engine { get; private set; } = IEvelyn.NewInstance;
        internal MockedLocalClient ClientA { get; private set; } = new MockedLocalClient();
        internal MockedLocalClient ClientB { get; private set; } = new MockedLocalClient();
        internal MockedConfigurator Configurator { get; private set; } = new MockedConfigurator();
        internal MockedClientService ClientService { get; private set; } = new MockedClientService();
        internal MockedManagementService ManagementService { get; private set; } = new MockedManagementService();
        internal NewOrder OrderA1 { get; private set; } = new NewOrder();
        internal NewOrder OrderA2 { get; private set; } = new NewOrder();
        internal NewOrder OrderA3 { get; private set; } = new NewOrder();
        internal NewOrder OrderB1 { get; private set; } = new NewOrder();
        internal Trade OrderA3Trade { get; private set; } = new Trade();
        internal Trade OrderB1Trade { get; private set; } = new Trade();

        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();

            Engine = IEvelyn.NewInstance;

            ClientA = new MockedLocalClient();
            ClientB = new MockedLocalClient();
            Configurator = new MockedConfigurator();

            /*
             * Engine is fully configured.
             */
            Engine.GenerateOHLC(new OHLCGenerator())
                .RegisterRemoteClient(ClientService)
                .RegisterLocalClient("MOCKED_CLIENT_A", ClientA, "l2205", "pp2205")
                .RegisterLocalClient("MOCKED_CLIENT_B", ClientB, "l2205")
                .RegisterManagement(ManagementService)
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

            /*
             * Exchange is connected.
             */
            Configurator.Broker.MockedConnect(true);
            Configurator.FeedSource.MockedConnect(true);

            /*
             * Feed source sends subscription respones.
             */
            Configurator.FeedSource.MockedReplySubscribe("l2205", new Description(), true);
            Configurator.FeedSource.MockedReplySubscribe("pp2205", new Description(), true);

            /*
             * Clients send order, broker sends response.
             * 
             * Client A
             * 1. Client sends order with default option, time condition and state condition.
             * 2. Feed source returns instrument status that triggers the order with state condition.
             * 3. Broker returns completed trade for the above order, with status Completed.
             * 
             * Client B
             * 1. Client sends order with some volume, triggered immediately.
             * 2. Broker returns part trade, with order status Trading.
             */
            OrderA1 = new NewOrder
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                TradingDay = DateOnly.MaxValue,
                TimeStamp = DateTime.MaxValue,
                OrderID = "MOCKED_ORDER_1",
                Price = 8888,
                Quantity = 7,
                Direction = Direction.Buy,
                Offset = Offset.Open,
            };

            OrderA2 = new NewOrder
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                TradingDay = DateOnly.MaxValue,
                TimeStamp = DateTime.MaxValue,
                OrderID = "MOCKED_ORDER_2",
                Price = 8889,
                Quantity = 5,
                Direction = Direction.Buy,
                Offset = Offset.Open,
            };

            OrderA3 = new NewOrder
            {
                InstrumentID = "pp2205",
                ExchangeID = "DCE",
                TradingDay = DateOnly.MaxValue,
                TimeStamp = DateTime.MaxValue,
                OrderID = "MOCKED_ORDER_3",
                Price = 8890,
                Quantity = 3,
                Direction = Direction.Buy,
                Offset = Offset.Open,
            };

            ClientA.MockedNewOrder(OrderA1);
            ClientA.MockedNewOrder(OrderA2,
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.Time,
                        Time = BaseTime.AddHours(1)
                    }
                });
            ClientA.MockedNewOrder(OrderA3,
                new OrderOption
                {
                    Trigger = new TriggerCondition
                    {
                        When = TriggerType.StateChange,
                        StateChange = InstrumentStatus.Continous
                    }
                });

            /*
             * Feed source returns instrument status triggering the state-change
             * condition, and broker receives the order request.
             * 
             * Then broker trades the order completely.
             */
            Configurator.FeedSource.MockedReceive(
                new Instrument
                {
                    InstrumentID = "pp2205",
                    ExchangeID = "DCE",
                    TradingDay = TradingDay,
                    Status = InstrumentStatus.Continous,
                    EnterTime = BaseTime
                });

            /*
             * Find the received two new order requests, replace the original order ID
             * with the rewritten ID.
             * 
             * The first request is by default option, and the second is triggered by state
             * change.
             */
            Assert.AreEqual(2, Configurator.Broker.ReceivedNewOrders.Count);

            /*
             * Trade the order of pp2205.
             */
            var newOrder = Configurator.Broker.ReceivedNewOrders.Where(received => received.InstrumentID == "pp2205").First();

            OrderA3Trade = new Trade
            {
                InstrumentID = "pp2205",
                ExchangeID = "DCE",
                TradingDay = TradingDay,
                TimeStamp = BaseTime,
                OrderID = newOrder.OrderID, /* Engine rewrites order ID. */
                Price = 8890,
                Quantity = 3,
                Direction = Direction.Buy,
                Offset = Offset.Open,
                TradeID = "MOCKED_ORDER_1_TRADE_1",
                TradePrice = 8890,
                TradeQuantity = 3,
                LeaveQuantity = 0,
                Status = OrderStatus.Completed,
                Message = "Completed"
            };
            Configurator.Broker.MockedTrade(OrderA3Trade,
                new Description
                {
                    Code = 0,
                    Message = "Order is completed."
                });

            /*
             * Client B sends order and trade a part.
             * 
             * Each client has ensured its own order ID distinct, but orders sent by
             * different clients can have the same ID.
             */
            OrderB1 = new NewOrder
            {
                InstrumentID = "l2205",
                ExchangeID = "DCE",
                TradingDay = DateOnly.MaxValue,
                TimeStamp = DateTime.MaxValue,
                OrderID = "MOCKED_ORDER_1", /* It is OK for client B to have the same ID with client A. */
                Price = 8892,
                Quantity = 7,
                Direction = Direction.Sell,
                Offset = Offset.Close,
            };

            ClientB.MockedNewOrder(OrderB1);

            var newOrderB = Configurator.Broker.ReceivedNewOrders.Last();

            /*
             * Ensure the last order is the one sent by client B.
             */
            Assert.AreEqual(8892, newOrderB.Price);
            Assert.AreEqual(Direction.Sell, newOrderB.Direction);
            Assert.AreEqual(Offset.Close, newOrderB.Offset);

            /*
             * Broker returns part trade for the above order.
             */
            OrderB1Trade = new Trade
            {
                InstrumentID = "l2205",
                TradingDay = TradingDay,
                TimeStamp = BaseTime,
                OrderID = newOrderB.OrderID, /* Engine rewrites order ID. */
                Price = 8892,
                Quantity = 3,
                Direction = Direction.Sell,
                Offset = Offset.Close,
                TradeID = "MOCKED_ORDER_1_TRADE_1",
                TradePrice = 8892,
                TradeQuantity = 1,
                LeaveQuantity = 2,
                Status = OrderStatus.Trading,
                Message = "Trading"
            };
            Configurator.Broker.MockedTrade(OrderB1Trade,
                new Description
                {
                    Code = 0,
                    Message = "Order is trading."
                });
        }

        [TestMethod("Query engine information when engine is fulfilled.")]
        public void QueryEngineInformation()
        {
            var query = ManagementService.Management.QueryEngineInformation();

            /*
             * Query returns no error.
             */
            Assert.AreEqual(0, query.Description.Code);
            Assert.AreEqual(string.Empty, query.Description.Message);

            /*
             * Broker is connected.
             */
            var broker = query.Result.Broker;

            Assert.AreEqual(true, broker.IsConnected);
            Assert.AreEqual(Configurator.Broker.TradingDay, broker.TradingDay);

            var feedSource = query.Result.FeedSource;

            /*
             * Feed source is connected.
             */
            Assert.AreEqual(true, feedSource.IsConnected);
            Assert.AreEqual(Configurator.FeedSource.TradingDay, feedSource.TradingDay);

            /*
             * Check feed source's OHLC generator.
             */
            Assert.AreEqual(1, feedSource.OHLCGeneratorCount);

            /*
             * Feed source has correct subscription responses.
             */
            Assert.AreEqual(2, feedSource.SubscriptionResponses.Count);

            Assert.IsTrue(feedSource.SubscriptionResponses["l2205"]);
            Assert.IsTrue(feedSource.SubscriptionResponses["pp2205"]);

            /*
             * Feed source has correct scheduled jobs.
             */
            Assert.AreEqual(1, feedSource.ScheduledJobs.Count);

            var job = feedSource.ScheduledJobs[0];

            Assert.AreEqual("MOCKED_CLIENT_A", job.ClientID);
            Assert.AreEqual("l2205", job.InstrumentID);
            Assert.AreEqual(TriggerType.Time, job.Option.Trigger.When);
            Assert.AreEqual(BaseTime.AddHours(1), job.Option.Trigger.Time);

            /*
             * Feed source has subscribed instruments.
             */
            Assert.AreEqual(2, feedSource.Instruments.Count);

            var l2205 = feedSource.Instruments.Where(instrument => instrument.InstrumentID == "l2205").First();
            var pp2205 = feedSource.Instruments.Where(instrument => instrument.InstrumentID == "pp2205").First();

            Assert.AreEqual(InstrumentStatus.Continous, l2205.Status);
            Assert.AreEqual(InstrumentStatus.Continous, pp2205.Status);
        }

        [TestMethod("Alter client to unsubscribe an instrument and query.")]
        public void AlterAndQueryClient()
        {
            /*
             * Query clients' information and check the following  pre condition.
             * 
             * 1. Client A subscribes for l2205 and pp2205.
             * 2. Client B subscribes for l2205.
             */
            var clients = ManagementService.Management.QueryClients().Result.Clients;

            /*
             * Client A subscribes for two instruments.
             */
            var clientA = clients.Where(client => client.ClientID == "MOCKED_CLIENT_A").First();

            Assert.AreEqual("MOCKED_CLIENT_A", clientA.ClientID);

            Assert.AreEqual(2, clientA.Subscription.Count);
            Assert.IsTrue(clientA.Subscription.Contains("l2205") && clientA.Subscription.Contains("pp2205"));

            /*
             * Client B subscribes for one instrument.
             */
            var clientB = clients.Where(client => client.ClientID == "MOCKED_CLIENT_B").First();

            Assert.AreEqual("MOCKED_CLIENT_B", clientB.ClientID);

            Assert.AreEqual(1, clientB.Subscription.Count);
            Assert.IsTrue(clientB.Subscription.Contains("l2205"));

            /*
             * pp2205 subscription counter decreases to zero, and removed from the engine.
             */
            var alter = ManagementService.Management.AlterClient("MOCKED_CLIENT_A", "l2205");

            Assert.AreEqual("MOCKED_CLIENT_A", alter.Result.ClientID);
            Assert.AreEqual(0, alter.Description.Code);
            Assert.AreEqual(string.Empty, alter.Description.Message);

            /*
             * Feed source return unsubscription response.
             */
            Configurator.FeedSource.MockedReplySubscribe("pp2205", new Description(), false);

            /*
             * Query the engine information and feed source has the correct subscription.
             */
            var feedSource = ManagementService.Management.QueryEngineInformation().Result.FeedSource;

            Assert.AreEqual(2, feedSource.SubscriptionResponses.Count);

            /*
             * l2205 is subscribed so it has value TRUE, and pp2205 has value FALSE.
             */
            Assert.IsTrue(feedSource.SubscriptionResponses["l2205"]);
            Assert.IsFalse(feedSource.SubscriptionResponses["pp2205"]);

            /*
             * Query clients' information again and shall reflect the subscription change.
             * 
             * Both clients subscribe for l2205, and no pp2205.
             */
            clients = ManagementService.Management.QueryClients().Result.Clients;

            clientA = clients.Where(client => client.ClientID == "MOCKED_CLIENT_A").First();

            Assert.AreEqual("MOCKED_CLIENT_A", clientA.ClientID);

            Assert.AreEqual(1, clientA.Subscription.Count);
            Assert.IsTrue(clientA.Subscription.Contains("l2205"));
        }

        [TestMethod("Query clients and client's order.")]
        public void QueryClientsAndOrder()
        {
            var clients = ManagementService.Management.QueryClients().Result.Clients;

            Assert.AreEqual(2, clients.Count);

            /*
             * Verify client A.
             */
            var clientA = clients.Where(client => client.ClientID == "MOCKED_CLIENT_A").First();

            Assert.AreEqual("MOCKED_CLIENT_A", clientA.ClientID);

            /*
             * Client A subscribes for two instruments.
             */
            Assert.AreEqual(2, clientA.Subscription.Count);
            Assert.IsTrue(clientA.Subscription.Contains("l2205") && clientA.Subscription.Contains("pp2205"));

            /*
             * Client A has 2 orders, and 1 order is scheduled and not triggered.
             */
            Assert.AreEqual(2, clientA.Orders.Count);

            /*
             * First order trades l2205.
             */
            var order1 = clientA.Orders.Where(brief => brief.Order.OrderID == "MOCKED_ORDER_1").First();
            var clientAOrder1 = ManagementService.Management.QueryClientOrder("MOCKED_CLIENT_A", "MOCKED_ORDER_1").Result;

            Assert.AreEqual("MOCKED_CLIENT_A", order1.ClientID);
            Assert.AreEqual(default(double), order1.AverageTradePrice);
            Assert.AreEqual(0, order1.TradeQuantity);
            Assert.AreEqual(default(DateTime), order1.LastTradeTime);
            Assert.AreEqual(OrderStatus.None, order1.Status);

            Assert.AreEqual(order1.ClientID, clientAOrder1.ClientID);
            Assert.AreEqual(order1.Status, clientAOrder1.Status);

            /*
             * Check order equality.
             */
            Assert.AreEqual(OrderA1, order1.Order);
            Assert.AreEqual(order1.Order, clientAOrder1.Order);

            /*
             * No trade.
             */
            Assert.AreEqual(0, clientAOrder1.Trades.Count);

            /*
             * Second order trades pp2205.
             */
            var order3 = clientA.Orders.Where(brief => brief.Order.OrderID == "MOCKED_ORDER_3").First();
            var clientAOrder3 = ManagementService.Management.QueryClientOrder("MOCKED_CLIENT_A", "MOCKED_ORDER_3").Result;

            Assert.AreEqual("MOCKED_CLIENT_A", order3.ClientID);
            Assert.AreEqual(8890, order3.AverageTradePrice);
            Assert.AreEqual(3, order3.TradeQuantity);
            Assert.AreEqual(BaseTime, order3.LastTradeTime);
            Assert.AreEqual(OrderStatus.Completed, order3.Status);

            Assert.AreEqual(order3.ClientID, clientAOrder3.ClientID);
            Assert.AreEqual(order3.Status, clientAOrder3.Status);

            /*
             * Check order equality.
             */
            Assert.AreEqual(OrderA3, order3.Order);
            Assert.AreEqual(order3.Order, clientAOrder3.Order);

            /*
             * Check 1 completed trade.
             */
            Assert.AreEqual(1, clientAOrder3.Trades.Count);

            /*
             * Rewrite order ID.
             */
            var trade = OrderA3Trade;
            trade.OrderID = "MOCKED_ORDER_3";
            Assert.AreEqual(trade, clientAOrder3.Trades[0]);

            /*
             * Client B has one order and is partly traded.
             */
            var clientB = clients.Where(client => client.ClientID == "MOCKED_CLIENT_B").First();

            Assert.AreEqual("MOCKED_CLIENT_B", clientB.ClientID);

            /*
             * Client B subscribes for 1 instrument.
             */
            Assert.AreEqual(1, clientB.Subscription.Count);
            Assert.IsTrue(clientB.Subscription.Contains("l2205"));

            /*
             * Client B has 1 order.
             */
            Assert.AreEqual(1, clientB.Orders.Count);

            var order4 = clientB.Orders.Where(brief => brief.Order.OrderID == "MOCKED_ORDER_1").First();
            var clientBOrder1 = ManagementService.Management.QueryClientOrder("MOCKED_CLIENT_B", "MOCKED_ORDER_1").Result;

            Assert.AreEqual("MOCKED_CLIENT_B", order4.ClientID);
            Assert.AreEqual(8892, order4.AverageTradePrice);
            Assert.AreEqual(1, order4.TradeQuantity);
            Assert.AreEqual(BaseTime, order4.LastTradeTime);
            Assert.AreEqual(OrderStatus.Trading, order4.Status);

            Assert.AreEqual(order4.ClientID, clientBOrder1.ClientID);
            Assert.AreEqual(order4.Status, clientBOrder1.Status);

            /*
             * Check order equality.
             */
            Assert.AreEqual(OrderB1, order4.Order);
            Assert.AreEqual(order4.Order, clientBOrder1.Order);

            /*
             * Client B has 1 trade.
             */
            Assert.AreEqual(1, clientBOrder1.Trades.Count);

            /*
             * Check trade.
             * Rewrite order ID.
             */
            trade = OrderB1Trade;
            trade.OrderID = "MOCKED_ORDER_1";
            Assert.AreEqual(trade, clientBOrder1.Trades[0]);
        }

        [TestMethod("Send command to client.")]
        public void SendCommand()
        {
            /*
             * Send command to client.
             * 
             * 1. Send command to correct client.
             * 2. Send command to an unknown client and receive error.
             * 3. Send command to a remote client and receive error.
             */
            var result = ManagementService.Management.SendCommand("MOCKED_CLIENT_A", "MOCKED_CLIENT_A_COMMAND");

            Assert.AreEqual(0, result.Description.Code);
            Assert.AreEqual("MOCKED_CLIENT_A_COMMAND", result.Result);

            /*
             * 2. Send command to an unknown client.
             */
            result = ManagementService.Management.SendCommand("UNKNOWN_CLIENT", "UNKNOWN_CLIENT_COMMAND");

            Assert.AreEqual(27, result.Description.Code);
            Assert.AreEqual(string.Empty, result.Result);

            /*
             * 3. Mock a remote client and send a command.
             */
            ClientService.GetClientOrCreate("REMOTE_CLIENT");

            result = ManagementService.Management.SendCommand("REMOTE_CLIENT", "REMOTE_CLIENT_COMMAND");

            Assert.AreEqual(26, result.Description.Code);
            Assert.AreEqual(string.Empty, result.Result);
        }
    }
}
