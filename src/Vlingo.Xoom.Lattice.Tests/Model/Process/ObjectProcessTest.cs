// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Lattice.Exchange;
using Vlingo.Xoom.Lattice.Exchange.Local;
using Vlingo.Xoom.Lattice.Model.Object;
using Vlingo.Xoom.Lattice.Model.Process;
using Vlingo.Xoom.Lattice.Tests.Model.Object;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Object;
using Vlingo.Xoom.Symbio.Store.Object.InMemory;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Lattice.Tests.Model.Process
{
    public class ObjectProcessTest
    {
        private readonly IExchange _exchange;
        private readonly ExchangeReceivers _exchangeReceivers;
        private readonly LocalExchangeSender _exchangeSender;
        private readonly World _world;

        [Fact]
        public void TestFiveStepEmittingProcess()
        {
            var process = _world.ActorFor<IFiveStepProcess>(() => new FiveStepEmittingObjectProcess());
            _exchangeReceivers.SetProcess(process);

            _exchange.Send(new DoStepOne());

            Assert.Equal(5, _exchangeReceivers.Access.ReadFrom<int>("stepCount"));

            Assert.Equal(5, process.QueryStepCount().Await());
        }

        public ObjectProcessTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("five-step-process-test");

            var queue = new AsyncMessageQueue(null);
            _exchange = new LocalExchange(queue);
            var adapter = new ProcessMessageTextAdapter();
            EntryAdapterProvider.Instance(_world).RegisterAdapter(adapter);

            var dispatcher = new MockDispatcher();
            var objectStore = _world.ActorFor<IObjectStore>(() => new InMemoryObjectStoreActor<string>(dispatcher));

            var objectTypeRegistry = new ObjectTypeRegistry(_world);

            var stepCountStateInfo =
                new Info<StepCountObjectState>(
                    objectStore,
                    nameof(StepCountObjectState),
                    MapQueryExpression.Using<StepCountObjectState>("find", MapQueryExpression.Map("id", "id")),
                    StateObjectMapper.With<StepCountObjectState>(new object(), new object()));

            objectTypeRegistry.Register(stepCountStateInfo);
            
            _exchangeSender = new LocalExchangeSender(queue);

            var processTypeRegistry = new ProcessTypeRegistry(_world);
            processTypeRegistry.Register(new ObjectProcessInfo<FiveStepEmittingObjectProcess, StepCountObjectState>(nameof(FiveStepEmittingObjectProcess), _exchange, objectTypeRegistry));
            
            _exchangeReceivers = new ExchangeReceivers();
            
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