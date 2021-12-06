// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Reactive.Streams;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Lattice.Exchange;
using Vlingo.Xoom.Lattice.Exchange.Local;
using Vlingo.Xoom.Lattice.Exchange.Streams;
using Vlingo.Xoom.Streams;
using Vlingo.Xoom.Streams.Sink.Test;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Lattice.Tests.Exchange.Stream
{
    public class LocalExchangeStreamTest : IDisposable
    {
        private readonly World _world;
        private IPublisher<LocalType1> _publisher;
        private IExchangeReceiver<LocalType1> _receiver;

        private readonly LocalType1 _local1 = new LocalType1("ABC", 123);

        [Fact]
        public void TestExchangeStreams()
        {
            var queue = new AsyncMessageQueue(null);
            CreateProxyWith(new ExchangeStreamSource<LocalType1>(false));
            var exchange = new LocalExchange(queue);

            exchange
                .Register(Covey<LocalType1, ExternalType1, LocalExchangeMessage>.Of(
                    new LocalExchangeSender(queue),
                    _receiver,
                    new LocalExchangeAdapter<LocalType1, ExternalType1>()));

            // subscribe to ExchangeStreamProxy with standard Subscriber and consume the message with a Sink
            var sink = new SafeConsumerSink<LocalType1>();
            var subscriber = _world.ActorFor<ISubscriber<LocalType1>>(() => new StreamSubscriber<LocalType1>(sink, 2));
            var access = sink.AfterCompleting(3);
            _publisher.Subscribe(subscriber);

            exchange.Send(_local1);

            var values = access.ReadFrom<List<LocalType1>>("values");
            Assert.Single(values);
            Assert.Equal(_local1.Attribute1, values[0].Attribute1);
            Assert.Equal(_local1.Attribute2, values[0].Attribute2);

            exchange.Close();
        }

        public LocalExchangeStreamTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("streams");
        }

        public void Dispose() => _world.Terminate();

        private void CreateProxyWith(ExchangeStreamSource<LocalType1> source)
        {
            var definition = Definition.Has<ExchangeStreamPublisher<LocalType1>>(Definition.Parameters(source,
                new PublisherConfiguration(5, Streams.Streams.OverflowPolicy.DropHead)));
            var protocols = _world.ActorFor(new[] { typeof(IPublisher<LocalType1>), typeof(IExchangeReceiver<LocalType1>) }, definition);
            _publisher = protocols.Get<IPublisher<LocalType1>>(0);
            _receiver = protocols.Get<IExchangeReceiver<LocalType1>>(1);
        }
    }
}