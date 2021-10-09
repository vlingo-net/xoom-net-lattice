// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Lattice.Grid.Hashring;

namespace Vlingo.Xoom.Lattice.Grid.Cache
{
    public abstract class HashedNodePoint<T> : HashedIdentity
    {
        public T NodeIdentifier { get; }
        
        protected HashedNodePoint(int hash, T nodeIdentifier) : base(hash) => NodeIdentifier = nodeIdentifier;

        public abstract void Excluded();

        public abstract void Included();

        public override string ToString() => $"HashedNodePoint[hash={Hash} nodeIdentifier={NodeIdentifier}]";
    }
}