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
        private ISpace[] partitions;
        private int totalPartitions;

        public PartitioningSpaceRouter(int totalPartitions, TimeSpan defaultScanInterval)
        {
            this.totalPartitions = totalPartitions;
            this.partitions = new ISpace[totalPartitions];

            Initialize(defaultScanInterval);
        }
        
        public ICompletes<T> ItemFor<T>(Type actorType, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public ICompletes<KeyItem<T>> Put<T>(IKey key, Item<T> item)
        {
            throw new NotImplementedException();
        }

        public ICompletes<Optional<KeyItem<T>>> Get<T>(IKey key, Period until)
        {
            throw new NotImplementedException();
        }

        public ICompletes<Optional<KeyItem<T>>> Take<T>(IKey key, Period until)
        {
            throw new NotImplementedException();
        }
        
        private void Initialize(TimeSpan defaultScanInterval)
        {
            for (var count = 0; count < totalPartitions; ++count)
            {
                //var definition = Definition.Has(SpaceActor.class, new SpaceInstantiator(defaultScanInterval), address().name() + "-" + count);
                //final Space internalSpace = childActorFor(Space.class, definition);
                //partitions[count] = internalSpace;
            }
        }

        // private Space spaceOf(final Key key) {
        //     final int partition = key.hashCode() % totalPartitions;
        //     return partitions[partition];
        // }
    }
}