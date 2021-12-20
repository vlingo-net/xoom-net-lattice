// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    public class PartitioningSpaceRouter : Actor, ISpace
    {
        private readonly ISpace[] _partitions;
        private readonly int _totalPartitions;

        public PartitioningSpaceRouter(int totalPartitions, TimeSpan defaultScanInterval)
        {
            _totalPartitions = totalPartitions;
            _partitions = new ISpace[totalPartitions];

            Initialize(defaultScanInterval);
        }
        
        public ICompletes<T> ItemFor<T>(Type actorType, params object[] parameters)
        {
            // Fail; not implemented. See SpaceItemFactoryRelay#itemFor.
            return Completes().With<T>(default!);
        }

        public ICompletes<KeyItem<T>> Put<T>(IKey key, Item<T> item)
        {
            var completes = CompletesEventually();
            SpaceOf(key).Put(key, item).AndThenConsume(keyItem => completes.With(keyItem));
            return Completes<KeyItem<T>>();
        }

        public ICompletes<Optional<KeyItem<T>>> Get<T>(IKey key, Period until)
        {
            var completes = CompletesEventually();
            SpaceOf(key).Get<T>(key, until).AndThenConsume(keyItem => completes.With(keyItem));
            return Completes<Optional<KeyItem<T>>>();
        }

        public ICompletes<Optional<KeyItem<T>>> Take<T>(IKey key, Period until)
        {
            var completes = CompletesEventually();
            SpaceOf(key).Take<T>(key, until).AndThenConsume(keyItem => completes.With(keyItem));
            return Completes<Optional<KeyItem<T>>>();
        }
        
        private void Initialize(TimeSpan defaultScanInterval)
        {
            for (var count = 0; count < _totalPartitions; ++count)
            {
                var definition = Definition.Has(() => new SpaceActor(defaultScanInterval), $"{Address.Name}-{count}");
                var internalSpace = ChildActorFor<ISpace>(definition);
                _partitions[count] = internalSpace;
            }
        }

        private ISpace SpaceOf(IKey key)
        {
            var partition = Math.Abs(key.GetHashCode()) % _totalPartitions;
            return _partitions[partition];
        }
    }
}