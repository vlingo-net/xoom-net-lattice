// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Lattice.Grid.Hashring;

namespace Vlingo.Xoom.Lattice.Grid.Cache;

public class CacheNodePoint<T> : HashedNodePoint<T>
{
    public Cache Cache { get; }

    public CacheNodePoint(Cache cache, int hash, T node) : base(hash, node) => Cache = cache;

    public override void Excluded()
    {
    }

    public override void Included()
    {
    }
}