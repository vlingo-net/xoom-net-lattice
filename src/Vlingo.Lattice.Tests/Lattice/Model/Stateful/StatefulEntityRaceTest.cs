// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Common.Serialization;
using Vlingo.Lattice.Model.Stateful;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Dispatch;
using Vlingo.Symbio.Store.State;
using Vlingo.Symbio.Store.State.InMemory;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Tests.Lattice.Model.Stateful
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
            var registry = new StatefulTypeRegistry<Entity1State>(_world);
            
            var store = _world.ActorFor<IStateStore<Entity1State>>(() => new InMemoryStateStoreActor<TextState, Entity1State>(new List<IDispatcher<Dispatchable<Entity1State, TextState>>> {_dispatcher}));
            
            registry.Register(new Info<Entity1State>(store, nameof(Entity1State)));
        }

        public void Dispose()
        {
            _world.Terminate();
        }
    }

    public class Entity1StateAdapter : StateAdapter<Entity1State, TextState>
    {
        public override int TypeVersion => 1;

        public override Entity1State FromRawState(TextState raw) => JsonSerialization.Deserialized<Entity1State>(raw.Data);

        public override TState FromRawState<TState>(TextState raw) => JsonSerialization.Deserialized<TState>(raw.Data);

        public override TextState ToRawState(Entity1State state, int stateVersion) => ToRawState(state.Id, state, stateVersion, Metadata.NullMetadata());

        public override TextState ToRawState(string id, Entity1State state, int stateVersion, Metadata metadata)
        {
            var serialization = JsonSerialization.Serialized(state);
            return new TextState(id, typeof(Entity1State), TypeVersion, serialization, stateVersion);
        }
    }
    
    public interface IEntity1
    {
        ICompletes<Entity1State> DefineWith(string name, int age);

        ICompletes<Entity1State> Current();

        void ChangeName(string name);

        void IncreaseAge();
    }

    public class Entity1State : BaseEntry<string>, IEquatable<Entity1State>
    {   
        public Entity1State(): this(UnknownId, EmptyTextData, -1)
        {
        }
        
        public Entity1State(string id, string name, int age) : base(id, typeof(string), 1, EmptyTextData)
        {
            Name = name;
            Age = age;
        }

        public Entity1State(string id) : this(id, null, 0)
        {
        }

        public string Name { get; }
        public int Age { get; }

        public bool HasState => Id != null && Name != null && Age > 0;

        public Entity1State Copy() => new Entity1State(Id, Name, Age);

        public override string ToString() => $"Entity1State[id={Id} name={Name} age={Age}]";

        public Entity1State WithName(string name) => new Entity1State(Id, name, Age);

        public Entity1State WithAge(int age) => new Entity1State(Id, Name, age);

        public override IEntry<string> WithId(string id) => new Entity1State(id);

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;

            var otherState = (Entity1State) obj;
            return Id.Equals(otherState.Id) && Name.Equals(otherState.Name) && Age == otherState.Age;
        }

        public bool Equals(Entity1State other) => Id == other.Id && Name == other.Name && Age == other.Age;

        public override int GetHashCode() => HashCode.Combine(Id, Name, Age);
    }

    public class Entity1Actor : StatefulEntity<Entity1State>, IEntity1
    {
        private readonly AtomicInteger _raceConditions;
        private readonly AtomicBoolean _runsApply = new AtomicBoolean(false);
        private Entity1State _state;

        public Entity1Actor(AtomicInteger raceConditions, string id) : base(id)
        {
            _raceConditions = raceConditions;
        }

        protected override ICompletes<TResult> Apply<TResult>(Entity1State state, string metadataValue, string operation, Func<TResult> andThen)
        {
            _runsApply.CompareAndSet(false, true);
            return base.Apply(state, metadataValue, operation, andThen);
        }

        protected override ICompletes<TResult> Apply<TSource, TResult>(Entity1State state, IEnumerable<Source<TSource>> sources, string metadataValue, string operation, Func<TResult> andThen)
        {
            _runsApply.CompareAndSet(false, true);
            return base.Apply(state, sources, metadataValue, operation, andThen);
        }

        protected override void AfterApply()
        {
            _runsApply.CompareAndSet(true, false);
            base.AfterApply();
        }

        protected override ICompletes Completes()
        {
            if (_runsApply.Get())
            {
                try
                {
                    Thread.Sleep(300);
                }
                catch (Exception e)
                {
                    Logger.Error("Tread sleep aborted", e);
                    throw;
                }
            }

            try
            {
                return base.Completes();
            }
            catch (Exception)
            {
                _raceConditions.IncrementAndGet();
                // Assert.assertNotNull("Race condition has been reproduced!", null);
                throw;
            }
        }

        //===================================
        // Entity1
        //===================================

        public ICompletes<Entity1State> DefineWith(string name, int age)
        {
            if (_state == null)
            {
                return Apply(new Entity1State(Id, name, age), "new", () => _state);
            }

            return Completes().With(_state.Copy());
        }

        public ICompletes<Entity1State> Current() => Completes().With(_state.Copy());

        public void ChangeName(string name) => Apply<Entity1State>(_state.WithName(name));

        public void IncreaseAge() => Apply<Entity1State>(_state.WithAge(_state.Age + 1));

        //===================================
        // StatefulEntity
        //===================================
        
        protected override void OnStateObject(Entity1State stateObject) => _state = stateObject;

        protected override void State(Entity1State state) => _state = state;
    }
}