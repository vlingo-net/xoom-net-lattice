// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Tests.Lattice.Query.Fixtures.Store;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Dispatch;
using Vlingo.Xoom.Symbio.Store.State;
using Vlingo.Xoom.Symbio.Store.State.InMemory;
using Xunit;
using Xunit.Abstractions;
using TestState = Vlingo.Tests.Lattice.Query.Fixtures.Store.TestState;

namespace Vlingo.Tests.Lattice.Query
{
    public class StateStoreQueryActorTest : IDisposable
    {
        private World world;
        private FailingStateStore stateStore;
        private ITestQueries queries;
        
        [Fact]
        public void ItFindsStateByIdAndType()
        {
            GivenTestState("1", "Foo");

            var testState = queries.TestStateById("1").Await(TimeSpan.FromMilliseconds(1000));

            Assert.Equal("Foo", testState.Name);
        }

        public StateStoreQueryActorTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            world = TestWorld.StartWithDefaults("test-state-store-query").World;
            
            // adapters has to be declared and configured before the store is instantiated
            var stateAdapterProvider = new StateAdapterProvider(world);
            stateAdapterProvider.RegisterAdapter(new TestStateAdapter());
            StateTypeStateStoreMap.StateTypeToStoreName(nameof(TestState), typeof(TestState));
            
            stateStore = new FailingStateStore(world.ActorFor<IStateStore>(() => new InMemoryStateStoreActor<TextState>(new NoOpDispatcher())));
            //StatefulTypeRegistry.RegisterAll(world, stateStore, typeof(TestState));
            queries = world.ActorFor<ITestQueries>(() => new TestQueriesActor(stateStore));
        }
        
        public void Dispose()
        {
            if (world != null)
            {
                world.Terminate();
            }
        }
        
        private void GivenStateReadFailures(int failures) => stateStore.ExpectReadFailures(failures);

        private void GivenTestState(string id, string name)
        {
            stateStore.Write(
                id,
                TestState.Named(name),
                1,
                new NoOpWriteResultInterest()
            );
        }
    }
    
    internal class NoOpWriteResultInterest : IWriteResultInterest
    {
        public void WriteResultedIn<TState, TSource>(IOutcome<StorageException, Result> outcome, string id, TState state, int stateVersion, IEnumerable<TSource> sources, object? @object)
        {
        }
    }
}