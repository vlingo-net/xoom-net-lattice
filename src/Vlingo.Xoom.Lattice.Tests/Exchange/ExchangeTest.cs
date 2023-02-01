// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Lattice.Exchange;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Lattice.Tests.Exchange;

public class ExchangeTest
{
    [Fact]
    public void TestThatExchangeSendsTyped()
    {
        var results = new ConcurrentQueue<object>();

        var logger = ConsoleLogger.TestInstance();
        var queue = new AsyncMessageQueue(null);
        var exchange = new TestExchange(queue, logger);
        var accessExchange = exchange.AfterCompleting(2);

        var exchangeReceiver1 = new TestExchangeReceiver1(results, logger);
        var accessExchangeReceiver1 = exchangeReceiver1.AfterCompleting(1);
        var exchangeReceiver2 = new TestExchangeReceiver2(results, logger);
        var accessExchangeReceiver2 = exchangeReceiver2.AfterCompleting(1);

        exchange
            .Register(Covey<LocalType1, ExternalType1, ExchangeMessage>.Of(
                new TestExchangeSender(queue, logger),
                exchangeReceiver1,
                new TestExchangeAdapter1()))
            .Register(Covey<LocalType2, ExternalType2, ExchangeMessage>.Of(
                new TestExchangeSender(queue, logger),
                exchangeReceiver2,
                new TestExchangeAdapter2()));

        var local1 = new LocalType1("ABC", 123);
        exchange.Send(local1);

        var local2 = new LocalType2("DEF", 456);
        exchange.Send(local2);

        Assert.Equal(2, accessExchange.ReadFrom<int>("sentCount"));
        Assert.Equal(local1, accessExchangeReceiver1.ReadFrom<LocalType1>("getMessage"));
        Assert.Equal(local2, accessExchangeReceiver2.ReadFrom<LocalType2>("getMessage"));

        exchange.Close();
    }

    public ExchangeTest(ITestOutputHelper output)
    {
        var converter = new Converter(output);
        Console.SetOut(converter);
    }
}