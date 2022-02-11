// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Cluster;
using Vlingo.Xoom.Lattice.Grid.Spaces;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Lattice.Tests.Grid.Spaces;

public class AccessorTest : IDisposable
{
    private readonly Vlingo.Xoom.Lattice.Grid.Grid _grid;
        
    [Fact]
    public void ShouldCreateAccessor()
    {
        var accessor1 = Accessor.Using(_grid, "test-accessor");
        Assert.NotNull(accessor1);
        Assert.True(accessor1.IsDefined);
        Assert.False(accessor1.IsNotDefined);
        var accessor2 = Accessor.Named(_grid, "test-accessor");
        Assert.NotNull(accessor2);
        Assert.Equal(accessor1.Name, accessor2.Name);
    }

    [Fact]
    public void ShouldCreateNamedSpace()
    {
        var accessor1 = Accessor.Using(_grid, "test-accessor");
        Assert.NotNull(accessor1.SpaceFor("test"));
    }

    [Fact]
    public void ShouldCreateNamedLongSweepIntervalSpace()
    {
        var accessor1 = Accessor.Using(_grid, "test-accessor");
        Assert.NotNull(accessor1.SpaceFor("test", 1000));
    }

    [Fact]
    public void ShouldCreateNamedDurationSweepIntervalSpace()
    {
        var accessor1 = Accessor.Using(_grid, "test-accessor");
        Assert.NotNull(accessor1.SpaceFor("test", 1, TimeSpan.FromMilliseconds(1000)));
    }

    public AccessorTest(ITestOutputHelper output)
    {
        var converter = new Converter(output);
        Console.SetOut(converter);
            
        _grid = Vlingo.Xoom.Lattice.Grid.Grid.Start("test-world", Configuration.Define(), ClusterProperties.OneNode(), "node1");
        _grid.QuorumAchieved();
    }

    public void Dispose() => _grid?.Terminate();
}