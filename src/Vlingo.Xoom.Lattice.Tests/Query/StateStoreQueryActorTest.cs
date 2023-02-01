// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Tests.Query.Fixtures.Store;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Dispatch;
using Vlingo.Xoom.Symbio.Store.State;
using Vlingo.Xoom.Symbio.Store.State.InMemory;
using Xunit;
using Xunit.Abstractions;
using TestState = Vlingo.Xoom.Lattice.Tests.Query.Fixtures.Store.TestState;

namespace Vlingo.Xoom.Lattice.Tests.Query;

public class StateStoreQueryActorTest : IDisposable
{
    private readonly World _world;
    private readonly FailingStateStore _stateStore;
    private readonly ITestQueries _queries;

    [Fact]
    public void ItFindsStateByIdAndType()
    {
        GivenTestState("1", "Foo");

        var testState = _queries.TestStateById("1").Await(TimeSpan.FromMilliseconds(1000));

        Assert.Equal("Foo", testState.Name);
    }

    [Fact]
    public void ItReturnsNullIfStateIsNotFoundByIdAndType()
    {
        var testState = _queries.TestStateById("1").Await(TimeSpan.FromMilliseconds(1000));

        Assert.Null(testState);
    }

    [Fact]
    public void ItFindsStateByIdAndTypeWithNotFoundState()
    {
        GivenTestState("1", "Foo");

        var testState = _queries.TestStateById("1", TestState.Missing()).Await(TimeSpan.FromMilliseconds(1000));

        Assert.Equal("Foo", testState.Name);
    }

    [Fact]
    public void ItReturnsNotFoundStateIfStateIsNotFoundByIdAndType()
    {
        var testState = _queries.TestStateById("1", TestState.Missing()).Await(TimeSpan.FromMilliseconds(1000));

        Assert.Equal(TestState.MISSING, testState.Name);
    }

    [Fact]
    public void ItFindsObjectStateByIdAndType()
    {
        GivenTestStateObject("1", "Foo");

        var testState = _queries.TestObjectStateById("1").Await(); // TimeSpan.FromMilliseconds(1000)

        Assert.True(testState.IsObject);
        Assert.Equal("Foo", testState.Data.Name);
    }

    [Fact]
    public void ItReturnsNullObjectStateIfNotFoundByIdAndType()
    {
        var testState = _queries.TestObjectStateById("1").Await(TimeSpan.FromMilliseconds(1000));

        Assert.Equal(ObjectState<TestState>.Null, testState);
    }

    [Fact]
    public void ItFindsObjectStateByIdAndTypeWithNotFoundObjectState()
    {
        GivenTestState("1", "Foo");

        var notFoundState = new ObjectState<TestState>();

        var testState = _queries.TestObjectStateById("1", notFoundState).Await(TimeSpan.FromMilliseconds(1000));

        Assert.True(testState.IsObject);
        Assert.Equal("Foo", testState.Data.Name);
    }

    [Fact]
    public void ItReturnsNullObjectStateIfNotFoundByIdAndTypeWithNotFoundObjectState()
    {
        var notFoundState = new ObjectState<TestState>();

        var testState = _queries.TestObjectStateById("1", notFoundState).Await(TimeSpan.FromMilliseconds(1000));

        Assert.Equal(notFoundState, testState);
    }

    [Fact]
    public void ItStreamsAllStatesByType()
    {
        GivenTestState("1", "Foo");
        GivenTestState("2", "Bar");
        GivenTestState("3", "Baz");
        GivenTestState("4", "Bam");

        var allStates = new List<TestState>();
        var testStates = _queries.All(allStates).Await(TimeSpan.FromMilliseconds(10000)).ToList();
        
        Assert.Equal(4, allStates.Count);
        Assert.Equal(4, testStates.Count);
        Assert.Equal(allStates, testStates);
        Assert.Equal("Foo", testStates[0].Name);
        Assert.Equal("Bar", testStates[1].Name);
        Assert.Equal("Baz", testStates[2].Name);
        Assert.Equal("Bam", testStates[3].Name);
    }

    [Fact]
    public void ItStreamsEmptyStore()
    {
        var allStates = new List<TestState>();
        var testStates = _queries.All(allStates).Await(TimeSpan.FromSeconds(10));
        
        Assert.Empty(allStates);
        Assert.Empty(testStates);
    }

    [Fact]
    public void ItFindsStateByIdAndTypeAfterRetries()
    {
        GivenTestState("1", "Foo");
        GivenStateReadFailures(3);

        var testState = _queries.TestStateById("1", 100, 3).Await(TimeSpan.FromMilliseconds(2000));

        Assert.Equal("Foo", testState.Name);
    }

    [Fact]
    public void ItFindsStateByIdAndTypeWithNotFoundStateAfterRetries()
    {
        GivenTestState("1", "Foo");
        GivenStateReadFailures(3);

        var testState = _queries.TestStateById("1", TestState.Missing(), 100, 3).Await(TimeSpan.FromMilliseconds(2000));

        Assert.Equal("Foo", testState.Name);
    }

    [Fact]
    public void ItReturnsNotFoundStateIfStateIsNotFoundByIdAndTypeAfterRetries()
    {
        GivenTestState("1", "Foo");
        GivenStateReadFailures(3);

        var testState = _queries.TestStateById("1", TestState.Missing(), 100, 2).Await(TimeSpan.FromMilliseconds(2000));

        Assert.Equal(TestState.MISSING, testState.Name);
    }

    [Fact]
    public void ItFindsObjectStateByIdAndTypeAfterRetries()
    {
        GivenTestStateObject("1", "Foo");
        GivenStateReadFailures(3);

        var testState = _queries.TestObjectStateById("1", 100, 3).Await(TimeSpan.FromMilliseconds(2000));

        Assert.True(testState.IsObject);
        Assert.Equal("Foo", testState.Data.Name);
    }

    [Fact]
    public void ItFindsObjectStateByIdAndTypeWithNotFoundStateAfterRetries()
    {
        GivenTestStateObject("1", "Foo");
        GivenStateReadFailures(3);

        var notFoundState = new ObjectState<TestState>();

        var testState = _queries.TestObjectStateById("1", notFoundState, 100, 3).Await(TimeSpan.FromMilliseconds(2000));

        Assert.True(testState.IsObject);
        Assert.Equal("Foo", testState.Data.Name);
    }

    [Fact]
    public void ItReturnsNullObjectStateIfStateIsNotFoundByIdAndTypeAfterRetries()
    {
        GivenTestStateObject("1", "Foo");
        GivenStateReadFailures(3);

        var notFoundState = new ObjectState<TestState>();

        var testState = _queries.TestObjectStateById("1", notFoundState, 100, 2).Await(TimeSpan.FromMilliseconds(2000));

        Assert.Equal(notFoundState, testState);
    }

    public StateStoreQueryActorTest(ITestOutputHelper output)
    {
        var converter = new Converter(output);
        Console.SetOut(converter);

        _world = TestWorld.StartWithDefaults("test-state-store-query").World;

        // adapters has to be declared and configured before the store is instantiated
        var stateAdapterProvider = new StateAdapterProvider(_world);
        stateAdapterProvider.RegisterAdapter(new TestStateAdapter());
        stateAdapterProvider.RegisterAdapter(new ObjectTestStateAdapter());
        StateTypeStateStoreMap.StateTypeToStoreName(nameof(TestState), typeof(TestState));
        StateTypeStateStoreMap.StateTypeToStoreName(nameof(ObjectState<TestState>), typeof(ObjectState<TestState>));

        _stateStore =
            new FailingStateStore(_world.ActorFor<IStateStore>(() =>
                new InMemoryStateStoreActor<TextState>(new NoOpDispatcher())));
        //StatefulTypeRegistry.RegisterAll(_world, _stateStore, typeof(ObjectState<TestState>));
        _queries = _world.ActorFor<ITestQueries>(() => new TestQueriesActor(_stateStore));
    }

    public void Dispose() => _world?.Terminate();

    private void GivenStateReadFailures(int failures) => _stateStore.ExpectReadFailures(failures);

    private void GivenTestState(string id, string name)
    {
        _stateStore.Write(
            id,
            TestState.NamedWithId(name, id),
            1,
            new NoOpWriteResultInterest()
        );
    }
        
    private void GivenTestStateObject(string id, string name)
    {
        _stateStore.Write(
            id,
            new ObjectState<TestState>("1", typeof(ObjectState<TestState>), 1, TestState.NamedWithId(name, id), 1),
            1,
            new NoOpWriteResultInterest()
        );
    }
}

internal class NoOpWriteResultInterest : IWriteResultInterest
{
    public void WriteResultedIn<TState, TSource>(IOutcome<StorageException, Result> outcome, string id,
        TState state, int stateVersion, IEnumerable<TSource> sources, object @object)
    {
    }
}