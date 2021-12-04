// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    internal class ExpirableItem<T> : IComparable<ExpirableItem<T>>
    {
        internal IKey Key { get; }
        internal T Object { get; }
        internal DateTime ExpiresOn { get; }
        internal Lease Lease { get; }

        internal ExpirableItem(IKey key, T @object, DateTime expiresOn, Lease lease)
        {
            Key = key;
            Object = @object;
            ExpiresOn = expiresOn;
            Lease = lease;
        }
        
        internal bool IsMaximumExpiration => ExpiresOn.GetCurrentSeconds() == DateTime.MaxValue.GetCurrentSeconds();
        
        public int CompareTo(ExpirableItem<T>? other) => Key.Compare(Key, other?.Key);
    }
}