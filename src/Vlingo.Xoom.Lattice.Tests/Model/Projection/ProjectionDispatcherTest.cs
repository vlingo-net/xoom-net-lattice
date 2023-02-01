// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Lattice.Model.Projection;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.Dispatch;
using Vlingo.Xoom.Symbio.Store.State;
using Vlingo.Xoom.Symbio.Store.State.InMemory;
using Xunit.Abstractions;
using IDispatcher = Vlingo.Xoom.Symbio.Store.Dispatch.IDispatcher;

namespace Vlingo.Xoom.Lattice.Tests.Model.Projection;

public abstract class ProjectionDispatcherTest : IDisposable
{
    protected readonly IDispatcher Dispatcher;
    protected IDispatcherControl DispatcherControl;
    protected readonly IProjectionDispatcher ProjectionDispatcher;
    protected readonly IStateStore Store;
    protected readonly World World;

    public ProjectionDispatcherTest(ITestOutputHelper output)
    {
        var converter = new Converter(output);
        Console.SetOut(converter);
            
        World = World.StartWithDefaults("test-store");

        var stateAdapterProvider = new StateAdapterProvider(World);
        stateAdapterProvider.RegisterAdapter(new Entity1StateAdapter());
        new EntryAdapterProvider(World);

        StateTypeStateStoreMap.StateTypeToStoreName(nameof(Entity1), typeof(Entity1));
        StateTypeStateStoreMap.StateTypeToStoreName(nameof(Entity2), typeof(Entity2));

        var dispatcherProtocols =
            World.Stage.ActorFor(
                new[] { DispatcherInterfaceType, typeof(IProjectionDispatcher) }, ProjectionDispatcherType);

        var dispatchers = Protocols.Two<IDispatcher, IProjectionDispatcher>(dispatcherProtocols);
        Dispatcher = dispatchers._1;
        ProjectionDispatcher = dispatchers._2;

        var storeProtocols =
            World.ActorFor(
                new[] {  StateStoreInterfaceType, typeof(IDispatcherControl) },
                typeof(InMemoryStateStoreActor<TextState>), Dispatcher);

        var storeWithControl = Protocols.Two<IStateStore, IDispatcherControl>(storeProtocols);
        Store = storeWithControl._1;
        DispatcherControl = storeWithControl._2;
    }
        
    protected abstract Type DispatcherInterfaceType  { get; set; }
    protected abstract Type ProjectionDispatcherType  { get; set; }
    protected abstract Type StateStoreInterfaceType { get; set; }

    public void Dispose() => World.Terminate();
}