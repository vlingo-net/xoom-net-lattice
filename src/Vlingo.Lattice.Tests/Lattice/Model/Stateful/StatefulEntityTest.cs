// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Lattice.Model.Stateful;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Dispatch;
using Vlingo.Symbio.Store.State;
using Vlingo.Symbio.Store.State.InMemory;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Tests.Lattice.Model.Stateful
{
    public class StatefulEntityTest : IDisposable
    {
        private static readonly AtomicInteger RaceConditions = new AtomicInteger(0);
        private readonly MockTextDispatcher _dispatcher;
        private readonly Random _idGenerator = new Random();
        private readonly World _world;
        private readonly StateAdapterProvider _stateAdapterProvider;

        [Fact]
        public void TestThatStatefulEntityPreservesRestores()
        {
            var entityId = $"{_idGenerator.Next(10_000)}";
            var state = new Entity1State(entityId, "Sally", 23);
            var access = _dispatcher.AfterCompleting(3);

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

            Assert.Equal(1, access.ReadFrom<int>("dispatchedStateCount"));
            var ids = access.ReadFrom<IEnumerable<string>>("dispatchedIds").ToList();
            Assert.Single(ids);

            var flatState = access.ReadFrom<string, TextState>("dispatchedState", ids[0]);
            Assert.Equal(new Entity1State(entityId, "Sally Jane", 24), _stateAdapterProvider.FromRaw<Entity1State, TextState>(flatState));

            restoredEntity1.Current().AndThenConsume(current => {
                Assert.Equal(new Entity1State(entityId, "Sally Jane", 24), current);
            });
        }
        
        [Fact]
        public void TestThatMetadataCallbackPreservesRestores()
        {
            var entityId = $"{_idGenerator.Next(10_000)}";
            var state = new Entity1State(entityId, "Sally", 23);
            var access = _dispatcher.AfterCompleting(3);

            var entity1 = _world.ActorFor<IEntity1>(() => new Entity1MetadataCallbackActor(RaceConditions, entityId));
            Assert.Equal(state, entity1.DefineWith(state.Name, state.Age).Await());
            Assert.Equal(state, entity1.Current().Await());

            entity1.ChangeName("Sally Jane");
            var newState = entity1.Current().Await();
            Assert.Equal("Sally Jane", newState.Name);

            entity1.IncreaseAge();
            newState = entity1.Current().Await();
            Assert.Equal(24, newState.Age);

            var restoredEntity1 = _world.ActorFor<IEntity1>(() => new Entity1MetadataCallbackActor(RaceConditions, entityId));
            var restoredEntity1State = restoredEntity1.Current().Await();
            Assert.NotNull(restoredEntity1State);

            Assert.Equal(1, access.ReadFrom<int>("dispatchedStateCount"));
            var ids = access.ReadFrom<IEnumerable<string>>("dispatchedIds").ToList();
            Assert.Single(ids);

            var flatState = access.ReadFrom<string, TextState>("dispatchedState", ids[0]);
            Assert.Equal(new Entity1State(entityId, "Sally Jane", 24), _stateAdapterProvider.FromRaw<Entity1State, TextState>(flatState));

            restoredEntity1.Current().AndThenConsume(current => {
                Assert.Equal(new Entity1State(entityId, "Sally Jane", 24), current);
            });
        }

        [Fact]
        public void ShouldDeserializeCorrectly()
        {
            var serialization =
                "{\"Name\":\"Sally Jane\",\"Age\":24,\"HasState\":true,\"None\":[],\"Id\":\"5474\",\"EntryData\":\"\",\"Metadata\":{\"Object\":{},\"OptionalObject\":{\"IsPresent\":true},\"Operation\":\"\",\"Value\":\"\",\"HasObject\":true,\"HasOperation\":false,\"HasValue\":false,\"IsEmpty\":true},\"TypeName\":\"System.String, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"TypeVersion\":1,\"EntryVersion\":-1,\"HasMetadata\":false,\"IsBinary\":false,\"IsObject\":false,\"IsText\":false,\"IsEmpty\":false,\"IsNull\":false,\"Typed\":\"System.String, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"Type\":\"System.String, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\"}";
            var serialization2 =
                "{\"Name\":\"Sally Jane\",\"Age\":24,\"Id\":\"5474\"}";
            var deserialized2 = JsonConvert.DeserializeObject<Entity1State>(serialization2);
            var deserialized = JsonConvert.DeserializeObject<Entity1State>(serialization);
        }
        
        public StatefulEntityTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("stateful-entity");
            _dispatcher = new MockTextDispatcher();

            _stateAdapterProvider = new StateAdapterProvider(_world);
            _stateAdapterProvider.RegisterAdapter(new Entity1StateAdapter());
            new EntryAdapterProvider(_world);
            var registry = new StatefulTypeRegistry<Entity1State>(_world);
            
            var store = _world.ActorFor<IStateStore<Entity1State>>(() => new InMemoryStateStoreActor<TextState, Entity1State>(new List<IDispatcher<Dispatchable<Entity1State, TextState>>> {_dispatcher}));
            
            registry.Register(new Info<Entity1State>(store, nameof(Entity1State)));
        }

        public void Dispose() => _world.Terminate();
    }
}