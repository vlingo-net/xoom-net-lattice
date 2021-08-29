// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Concurrent;
using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Lattice.Exchange;
using Vlingo.Xoom.Lattice.Exchange.Local;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Exchange.Local
{
    public class LocalExchangeTest
    {
        [Fact]
        public void TestThatExchangeSendsTyped()
        {
            var results = new ConcurrentQueue<object>();

            var logger = ConsoleLogger.TestInstance();
            var queue = new AsyncMessageQueue(null);
            var exchange = new LocalExchange(queue);

            var receiver1 = new TestExchangeReceiver1(results, logger);
            var access1 = receiver1.AfterCompleting(1);

            var receiver2 = new TestExchangeReceiver2(results, logger);
            var access2 = receiver2.AfterCompleting(1);

            exchange
                .Register(Covey<LocalType1, ExternalType1, LocalExchangeMessage>.Of(
                    new LocalExchangeSender(queue),
                    receiver1,
                    new LocalExchangeAdapter<LocalType1, ExternalType1>()))
                .Register(Covey<LocalType2, ExternalType2, LocalExchangeMessage>.Of(
                    new LocalExchangeSender(queue),
                    receiver2,
                    new LocalExchangeAdapter<LocalType2, ExternalType2>()));

            var local1 = new LocalType1("ABC", 123);
            exchange.Send(local1);

            var local2 = new LocalType2("DEF", 456);
            exchange.Send(local2);

            Assert.Equal(local1, access1.ReadFrom<LocalType1>("getMessage"));
            Assert.Equal(local2, access2.ReadFrom<LocalType2>("getMessage"));

            exchange.Close();
        }
    }
}