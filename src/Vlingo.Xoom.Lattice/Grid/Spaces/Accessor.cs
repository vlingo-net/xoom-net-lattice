// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Grid.Spaces;

public class Accessor
{
    private static readonly long DefaultScanInterval = 15_000;
    private static readonly int DefaultTotalPartitions = 5;
    private static readonly float DistributedWriteThroughFactor = 0.5f;
        
    private static readonly Accessor NullAccessor = new Accessor(null, null);
    private readonly Grid? _grid;
    private readonly ConcurrentDictionary<string, ISpace> _spaces = new ConcurrentDictionary<string, ISpace>();
    private readonly ConcurrentDictionary<string, IDistributedSpace> _distributedSpaces = new ConcurrentDictionary<string, IDistributedSpace>();
    private static volatile object _syncLock = new object();
        
    public string? Name { get; }

    public static Accessor Named(Grid grid, string name)
    {
        lock (_syncLock)
        {
            var accessor = grid.World.ResolveDynamic<Accessor>(name);

            if (accessor == null)
            {
                accessor = NullAccessor;
            }

            return accessor;
        }
    }
        
    public static Accessor Using(Grid grid, string name)
    {
        lock (_syncLock)
        {
            var accessor = grid.World.ResolveDynamic<Accessor>(name);

            if (accessor == null)
            {
                accessor = new Accessor(grid, name);
                grid.World.RegisterDynamic(name, accessor);
            }

            return accessor;   
        }
    }
        
    public bool IsDefined => _grid != null && Name != null;

    public bool IsNotDefined => !IsDefined;
        
    public ISpace DistributedSpaceFor(string spaceName) => 
        DistributedSpaceFor(spaceName, DefaultTotalPartitions, TimeSpan.FromMilliseconds(DefaultScanInterval));

    public ISpace DistributedSpaceFor(string spaceName, int totalPartitions) => 
        DistributedSpaceFor(spaceName, totalPartitions, TimeSpan.FromMilliseconds(DefaultScanInterval));

    public ISpace DistributedSpaceFor(string spaceName, int totalPartitions, TimeSpan scanInterval)
    {
        if (string.IsNullOrEmpty(Name))
        {
            throw new ArgumentNullException(nameof(Name), "The Name must be defined first.");
        }
            
        if (_grid == null)
        {
            throw new ArgumentNullException(nameof(_grid), "The Grid must be defined first.");
        }
            
        lock (_syncLock)
        {
            if (!_distributedSpaces.TryGetValue(Name!, out var distributedSpace))
            {
                var localStage = _grid.LocalStage();
                var localSpace = SpaceFor(spaceName, totalPartitions, scanInterval);
                var definition = Definition.Has(() =>
                    new DistributedSpaceActor(Name!, spaceName, totalPartitions, scanInterval, DistributedWriteThroughFactor, localSpace, _grid));
                distributedSpace = localStage.ActorFor<IDistributedSpace>(definition);
                _distributedSpaces.AddOrUpdate(Name!, key => distributedSpace, (s, space) => distributedSpace);
            }

            return distributedSpace!;
        }
    }
        
    public ISpace SpaceFor(string spaceName) {
        return SpaceFor(spaceName, DefaultTotalPartitions, TimeSpan.FromMilliseconds(DefaultScanInterval));
    }

    public ISpace SpaceFor(string spaceName, int totalPartitions) => 
        SpaceFor(spaceName, totalPartitions, TimeSpan.FromMilliseconds(DefaultScanInterval));

    public ISpace SpaceFor(string spaceName, long scanInterval) => 
        SpaceFor(spaceName, DefaultTotalPartitions, TimeSpan.FromMilliseconds(scanInterval));

    public ISpace SpaceFor(string spaceName, int totalPartitions, long scanInterval) => 
        SpaceFor(spaceName, totalPartitions, TimeSpan.FromMilliseconds(scanInterval));

    public ISpace SpaceFor(string spaceName, int totalPartitions, TimeSpan scanInterval)
    {
        if (scanInterval <= TimeSpan.Zero)
        {
            throw new ArgumentException("The scanInterval must be greater than zero.");
        }

        if (!IsDefined)
        {
            throw new InvalidOperationException("Accessor is invalid.");
        }

        var spaceExist = _spaces.TryGetValue(spaceName, out var space);

        if (!spaceExist)
        {
            var definition = Definition.Has(() => new PartitioningSpaceRouter(totalPartitions, scanInterval), spaceName);
            var internalSpace = _grid?.ActorFor<ISpace>(definition);
            space = new SpaceItemFactoryRelay(_grid!, internalSpace!);
            _spaces.AddOrUpdate(spaceName, s => space, (s, update) => space);
        }

        return space!;
    }

    private Accessor(Grid? grid, string? name)
    {
        _grid = grid;
        Name = name;
    }
}