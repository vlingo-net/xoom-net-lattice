// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Common.Message;
using Vlingo.Lattice.Exchange;
using Vlingo.Lattice.Exchange.Local;
using Vlingo.Lattice.Model.Process;
using Vlingo.Lattice.Model.Stateful;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.State;
using Vlingo.Xoom.Symbio.Store.State.InMemory;
using Vlingo.Tests.Lattice.Model.Stateful;
using Vlingo.Xoom.Actors;
using Xunit;
using Xunit.Abstractions;
using IDispatcher = Vlingo.Xoom.Symbio.Store.Dispatch.IDispatcher;

namespace Vlingo.Tests.Lattice.Model.Process
{
    public class StatefulProcessTest : IDisposable
    {
        private readonly IExchange _exchange;
        private readonly ExchangeReceivers _exchangeReceivers;
        private readonly LocalExchangeSender _exchangeSender;
        private readonly MockTextDispatcher _dispatcher;
        private readonly World _world;

        [Fact]
        public void TestFiveStepEmittingProcess()
        {
            var process = _world.ActorFor<IFiveStepProcess>(() => new FiveStepEmittingStatefulProcess());
            _exchangeReceivers.SetProcess(process);
            _dispatcher.AfterCompleting(4);
            
            _exchange.Send(new DoStepOne());

            Assert.Equal(5, _exchangeReceivers.Access.ReadFrom<int>("stepCount"));

            Assert.Equal(5, process.QueryStepCount().Await());
        }

        public StatefulProcessTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("five-step-process-test");

            var queue = new AsyncMessageQueue(null);
            _exchange = new LocalExchange(queue);
            _dispatcher = new MockTextDispatcher();
            var stateStore = _world.ActorFor<IStateStore>(() => new InMemoryStateStoreActor<TextState, TextEntry>(new List<IDispatcher> {_dispatcher}));

            var statefulTypeRegistry = new StatefulTypeRegistry(_world);
            
            var stepCountStateInfo = new Vlingo.Lattice.Model.Stateful.Info(stateStore, typeof(StepCountState), stateStore.GetType().Name);
            statefulTypeRegistry.Register(stepCountStateInfo);

            _exchangeReceivers = new ExchangeReceivers();
            _exchangeSender = new LocalExchangeSender(queue);

            var processTypeRegistry = new ProcessTypeRegistry(_world);
            processTypeRegistry.Register(new StatefulProcessInfo<FiveStepEmittingStatefulProcess, StepCountState>(nameof(FiveStepEmittingStatefulProcess), _exchange, statefulTypeRegistry));

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

        public void Dispose() => _world.Terminate();
    }
}