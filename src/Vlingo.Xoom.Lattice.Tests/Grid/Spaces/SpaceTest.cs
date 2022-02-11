// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Cluster;
using Vlingo.Xoom.Lattice.Grid.Spaces;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Lattice.Tests.Grid.Spaces;

public class SpaceTest : IDisposable
{
    private const string DefaultItem = "ThisIsAnItem";
    private readonly Vlingo.Xoom.Lattice.Grid.Grid _grid;
        
    [Fact]
    public void ShouldCreateSpace()
    {
        var accessor1 = Accessor.Using(_grid, "test-create");
        Assert.NotNull(accessor1.SpaceFor("test"));
    }

    [Fact]
    public void ShouldPutItemInSpace()
    {
        var accessor1 = Accessor.Using(_grid, "test-put");
        var space = accessor1.SpaceFor("test");
        var key1 = new Key1("123");
        space.Put(key1, Item.Of(DefaultItem, Lease.Forever));
        Pause(1);
        var item = space.Get(key1, Period.Forever).Await();
        Assert.True(item.IsPresent);
        Assert.Equal(item.Get().Object, DefaultItem);
    }

    [Fact]
    public void ShouldSweepItemAndEvictQueryFromSpace()
    {
        var accessor1 = Accessor.Using(_grid, "test-sweep-evict");
        var space = accessor1.SpaceFor("test", 1, TimeSpan.FromMilliseconds(100));
        var key1 = new Key1("123");
        space.Put(key1, Item.Of(DefaultItem, Lease.Of(TimeSpan.Zero)));
        Pause(1);
        var item = space.Get(key1, Period.None).Await();
        Assert.False(item.IsPresent);
    }

    [Fact]
    public void ShouldFindItemAfterGetSpace()
    {
        var accessor1 = Accessor.Using(_grid, "test-find-after");
        var space = accessor1.SpaceFor("test", 1, TimeSpan.FromMilliseconds(1_000));
        var key1 = new Key1("123");
        var completes = space.Get(key1, Period.Of(10000));
        Pause(1);
        space.Put(key1, Item.Of(DefaultItem, Lease.Of(TimeSpan.FromMilliseconds(1_000))));
        var item = completes.Await();
        Assert.True(item.IsPresent);
        Assert.Equal(item.Get().Object, DefaultItem);
    }

    [Fact] 
    public void ShouldFailGetItemAfterTakeSpace()
    {
        var accessor1 = Accessor.Using(_grid, "test-take");
        var space = accessor1.SpaceFor("take-test", 1, TimeSpan.FromMilliseconds(1_000));
        var key1 = new Key1("123");
        var completes = space.Take(key1, Period.Of(1_000));
        space.Put(key1, Item.Of(DefaultItem, Lease.Forever));
        var item = completes.Await();
        Assert.True(item.IsPresent);
        Assert.Equal(item.Get().Object, DefaultItem);
        var noItem = space.Get(key1, Period.None).Await();
        Assert.False(noItem.IsPresent);
    }

    public SpaceTest(ITestOutputHelper output)
    {
        var converter = new Converter(output);
        Console.SetOut(converter);
            
        _grid = Vlingo.Xoom.Lattice.Grid.Grid.Start("test-world", Configuration.Define(), ClusterProperties.OneNode(), "node1");
        _grid.QuorumAchieved();
    }
        
    public void Dispose() => _grid?.Terminate();
        
    private void Pause(int seconds)
    {
        try
        {
            Thread.Sleep(seconds * 1_000);
        }
        catch (Exception)
        {
            // ignore
        }
    }
}