// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    [Serializable]
    public class Item : IScheduledScannable<Item>
    {
        public Lease Lease { get; }
        public object Object { get; }

        public static Item Of(object @object, Lease lease) => new Item(@object, lease);

        public Item Scannable() => this;

        protected Item(object @object, Lease lease)
        {
            Object = @object;
            Lease = lease;
        }
    }
}