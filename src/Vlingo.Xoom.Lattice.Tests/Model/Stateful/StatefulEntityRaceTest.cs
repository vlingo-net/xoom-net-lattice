// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model.Stateful;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.State;
using Vlingo.Xoom.Symbio.Store.State.InMemory;
using Xunit;
using Xunit.Abstractions;
using IDispatcher = Vlingo.Xoom.Symbio.Store.Dispatch.IDispatcher;

namespace Vlingo.Xoom.Lattice.Tests.Model.Stateful
{
    public class StatefulEntityRaceTest : IDisposable
    {
        private static readonly AtomicInteger RaceConditions = new AtomicInteger(0);
        private readonly MockTextDispatcher _dispatcher;
        private readonly Random _idGenerator = new Random();
        private readonly World _world;
        
        [Fact]
        public void TestThatStatefulEntityPreservesRestores()
        {
            RaceConditions.Set(0);

            var entityId = $"{_idGenerator.Next(10_000)}";
            var state = new Entity1State(entityId, "Sally", 23);

            var entity1 = _world.ActorFor<IEntity1>(() => new Entity1Actor(RaceConditions, entityId));
            Assert.Equal(state, entity1.DefineWith(state.Name, state.Age).Await());
            Assert.Equal(state, entity1.Current().Await());

            entity1.ChangeName("Sally Jane");
            var newState = entity1.Current().Await();
            Assert.Equal("Sally Jane", newState.Name);

            entity1.IncreaseAge();
            newState = entity1.Current().Await();
            Assert.Equal(24, newState.Age);

            var restoredEntity1 = _world.ActorFor<IEntity1>(() => new Entity1Actor(RaceConditions, entityId));
            var restoredEntity1State = restoredEntity1.Current().Await();
            Assert.NotNull(restoredEntity1State);

            // check whether race conditions have been reproduced
            Assert.Equal(0, RaceConditions.Get());
        }

        public StatefulEntityRaceTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("stateful-entity");
            _dispatcher = new MockTextDispatcher();

            var stateAdapterProvider = new StateAdapterProvider(_world);
            stateAdapterProvider.RegisterAdapter(new Entity1StateAdapter());
            new EntryAdapterProvider(_world);
            var registry = new StatefulTypeRegistry(_world);
            
            var store = _world.ActorFor<IStateStore>(() => new InMemoryStateStoreActor<TextState>(new List<IDispatcher> {_dispatcher}));
            
            registry.Register(new Info(store, typeof(Entity1State), nameof(Entity1State)));
        }

        public void Dispose() => _world.Terminate();
    }
}