// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Common.Message;
using Vlingo.Lattice.Exchange;
using Vlingo.Lattice.Exchange.Local;
using Vlingo.Lattice.Model.Object;
using Vlingo.Lattice.Model.Process;
using Vlingo.Symbio;
using Vlingo.Symbio.Store;
using Vlingo.Symbio.Store.Object;
using Vlingo.Symbio.Store.Object.InMemory;
using Vlingo.Tests.Lattice.Model.Object;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Tests.Lattice.Model.Process
{
    public class ObjectProcessTest
    {
        private readonly IExchange _exchange;
        private readonly ExchangeReceivers _exchangeReceivers;
        private readonly LocalExchangeSender _exchangeSender;
        private readonly IFiveStepProcess _process;

        [Fact]
        public void TestFiveStepEmittingProcess()
        {
            _exchange.Send(new DoStepOne());

            Assert.Equal(5, _exchangeReceivers.Access.ReadFrom<int>("stepCount"));

            Assert.Equal(5, _process.QueryStepCount().Await());
        }

        public ObjectProcessTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            var world = World.StartWithDefaults("five-step-process-test");

            var queue = new AsyncMessageQueue(null);
            _exchange = new LocalExchange(queue);
            var adapter = new ProcessMessageTextAdapter();
            EntryAdapterProvider.Instance(world).RegisterAdapter(adapter);

            var dispatcher = new MockDispatcher();
            var objectStore = world.ActorFor<IObjectStore>(() => new InMemoryObjectStoreActor<string>(dispatcher));

            var objectTypeRegistry = new ObjectTypeRegistry(world);

            var stepCountStateInfo =
                new Vlingo.Lattice.Model.Object.Info<StepCountObjectState>(
                    objectStore,
                    nameof(StepCountObjectState),
                    MapQueryExpression.Using<StepCountObjectState>("find", MapQueryExpression.Map("id", "id")),
                    StateObjectMapper.With<StepCountObjectState>(new object(), new object()));

            objectTypeRegistry.Register(stepCountStateInfo);
            
            _exchangeSender = new LocalExchangeSender(queue);

            var processTypeRegistry = new ProcessTypeRegistry<StepCountObjectState>(world);
            processTypeRegistry.Register(new ObjectProcessInfo<StepCountObjectState>(nameof(FiveStepEmittingObjectProcess), _exchange, objectTypeRegistry));
            
            _process = world.ActorFor<IFiveStepProcess>(() => new FiveStepEmittingObjectProcess());
            _exchangeReceivers = new ExchangeReceivers(_process);
            
            RegisterExchangeCoveys();
        }
        
        private void RegisterExchangeCoveys()
        {
            _exchange
                .Register(Covey<DoStepOne, DoStepOne, LocalExchangeMessage>.Of(
                    _exchangeSender,
                    _exchangeReceivers.DoStepOneReceiver,
                    new LocalExchangeAdapter<DoStepOne, DoStepOne>()))
            .Register(Covey<DoStepTwo, DoStepTwo, LocalExchangeMessage>.Of(
                _exchangeSender,
                _exchangeReceivers.DoStepTwoReceiver,
                new LocalExchangeAdapter<DoStepTwo, DoStepTwo>()))
            .Register(Covey<DoStepThree, DoStepThree, LocalExchangeMessage>.Of(
                _exchangeSender,
                _exchangeReceivers.DoStepThreeReceiver,
                new LocalExchangeAdapter<DoStepThree, DoStepThree>()))
            .Register(Covey<DoStepFour, DoStepFour, LocalExchangeMessage>.Of(
                _exchangeSender,
                _exchangeReceivers.DoStepFourReceiver,
                new LocalExchangeAdapter<DoStepFour, DoStepFour>()))
            .Register(Covey<DoStepFive, DoStepFive, LocalExchangeMessage>.Of(
                _exchangeSender,
                _exchangeReceivers.DoStepFiveReceiver,
                new LocalExchangeAdapter<DoStepFive, DoStepFive>()));
        }
    }
}