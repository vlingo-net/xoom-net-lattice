// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    public class Accessor
    {
        private static readonly long DefaultScanInterval = 15_000;
        private static readonly int DefaultTotalPartitions = 5;
        
        private static readonly Accessor NullAccessor = new Accessor(null, null);
        private readonly Grid? _grid;
        private readonly ConcurrentDictionary<string, ISpace> _spaces = new ConcurrentDictionary<string, ISpace>();
        private static volatile object _syncLock = new object();
        
        public string? Name { get; }

        public static Accessor Named(Grid grid, string name)
        {
            var accessor = grid.World.ResolveDynamic<Accessor>(name);

            if (accessor == null)
            {
                accessor = NullAccessor;
            }

            return accessor;
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
        
        public ISpace SpaceFor(string name) {
            return SpaceFor(name, DefaultTotalPartitions, TimeSpan.FromMilliseconds(DefaultScanInterval));
        }

        public ISpace SpaceFor(string name, int totalPartitions) => 
            SpaceFor(name, totalPartitions, TimeSpan.FromMilliseconds(DefaultScanInterval));

        public ISpace SpaceFor(string name, long defaultScanInterval) => 
            SpaceFor(name, DefaultTotalPartitions, TimeSpan.FromMilliseconds(defaultScanInterval));

        public ISpace SpaceFor(string name, int totalPartitions, long defaultScanInterval) => 
            SpaceFor(name, totalPartitions, TimeSpan.FromMilliseconds(defaultScanInterval));

        public ISpace SpaceFor(string name, int totalPartitions, TimeSpan defaultScanInterval)
        {
            if (defaultScanInterval <= TimeSpan.Zero)
            {
                throw new ArgumentException("The defaultScanInterval must be greater than zero.");
            }

            if (!IsDefined)
            {
                throw new InvalidOperationException("Accessor is invalid.");
            }

            var spaceExist = _spaces.TryGetValue(name, out var space);

            if (!spaceExist)
            {
                var definition = Definition.Has(() => new PartitioningSpaceRouter(totalPartitions, defaultScanInterval), name);
                var internalSpace = _grid?.ActorFor<ISpace>(definition);
                space = new SpaceItemFactoryRelay(_grid!, internalSpace!);
                _spaces.AddOrUpdate(name, s => space, (s, update) => space);
            }

            return space!;
        }

        private Accessor(Grid? grid, string? name)
        {
            _grid = grid;
            Name = name;
        }
    }
}