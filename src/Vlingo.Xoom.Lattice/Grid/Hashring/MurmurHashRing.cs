// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;

namespace Vlingo.Xoom.Lattice.Grid.Hashring;

public abstract class MurmurHashRing<T> : IHashRing<T>
{
    protected readonly Func<int, T, HashedNodePoint<T>> Factory;
    protected readonly MurmurHash3 Hasher;
    protected readonly int PointsPerNode;

    public MurmurHashRing(int pointsPerNode, Func<int, T, HashedNodePoint<T>> factory, uint seed)
    {
        PointsPerNode = pointsPerNode;
        Factory = factory;
        Hasher = new MurmurHash3(seed);
    }

    protected int Hashed(object id)
    {
        using var stream = new MemoryStream(ByteConverter.ConvertToByteArray(id)!);
        return Hasher.Hash(stream);
    }
        
    protected HashedNodePoint<T> HashedNodePointOf(object id)
    {
        var hashedNodePointOf = Factory(Hashed(id), default!);
        return hashedNodePointOf;
    }

    public abstract void Dump();

    public abstract IHashRing<T> ExcludeNode(T nodeIdentifier);

    public abstract IHashRing<T> IncludeNode(T nodeIdentifier);

    public abstract T NodeOf(object id);

    public abstract IHashRing<T> Copy();
}