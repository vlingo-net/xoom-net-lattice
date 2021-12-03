// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    public class Item<T> : IScheduledScannable<Item<T>>
    {
        public Lease Lease { get; }
        public T Object { get; }

        public static Item<T> Of(T @object, Lease lease) => new Item<T>(@object, lease);

        public Item<T> Scannable() => this;

        protected Item(T @object, Lease lease)
        {
            Object = @object;
            Lease = lease;
        }
    }
}