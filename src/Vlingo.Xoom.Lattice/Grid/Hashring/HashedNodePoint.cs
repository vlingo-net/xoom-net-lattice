// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Xoom.Lattice.Grid.Hashring
{
    public abstract class HashedNodePoint<T> : HashedIdentity
    {
        public T NodeIdentifier { get; }
        
        protected HashedNodePoint(int hash, T nodeIdentifier) : base(hash) => NodeIdentifier = nodeIdentifier;

        public abstract void Excluded();

        public abstract void Included();

        public override string ToString() => $"HashedNodePoint[hash={Hash} nodeIdentifier={NodeIdentifier}]";
    }
    
    public class HashNodePointComparer<T> : IComparer<HashedNodePoint<T>>
    {
        public int Compare(HashedNodePoint<T>? x, HashedNodePoint<T>? y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            return x.Hash.CompareTo(y.Hash);
        }
    }
}