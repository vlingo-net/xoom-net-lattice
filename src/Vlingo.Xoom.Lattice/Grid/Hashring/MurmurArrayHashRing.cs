// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Grid.Hashring
{
    public class MurmurArrayHashRing<T> : MurmurHashRing<T>
    {
        private HashedNodePoint<T>[] _hashedNodePoints;
        private static uint DefaultSeed = 31;
        
        public MurmurArrayHashRing(int pointsPerNode, Func<int, T, HashedNodePoint<T>> factory) : base(pointsPerNode, factory, DefaultSeed)
            => _hashedNodePoints = Array.Empty<HashedNodePoint<T>>();

        public override void Dump()
        {
            Console.WriteLine($"NODES: {_hashedNodePoints.Length}");
            foreach (var hashedNodePoint in _hashedNodePoints)
            {
                Console.WriteLine($"NODE: {hashedNodePoint}");
            }
        }

        public override IHashRing<T> ExcludeNode(T nodeIdentifier)
        {
            var exclusive = Less();
            var index = 0;
            var element = 0;
            var hash = Hashed(nodeIdentifier!.ToString() + element);
            foreach (var hashedNodePoint in _hashedNodePoints)
            {
                if (hashedNodePoint.Hash == hash)
                {
                    hash = Hashed(nodeIdentifier.ToString() + ++element);
                } 
                else
                {
                    exclusive[index++] = hashedNodePoint;
                }
            }

            _hashedNodePoints = exclusive;

            return this;
        }

        public override IHashRing<T> IncludeNode(T nodeIdentifier)
        {
            var startingAt = MoreStartingAt();
            for (var element = startingAt; element < _hashedNodePoints.Length; ++element)
            {
                var hash = Hashed(nodeIdentifier!.ToString() + element);
                _hashedNodePoints[element] = Factory(hash, nodeIdentifier);
            }
            Array.Sort(_hashedNodePoints, new HashNodePointComparer<T>());

            return this;
        }

        public override T NodeOf(object id)
        {
            var hashedNodePoint = HashedNodePointOf(id);
            var index = Array.BinarySearch(_hashedNodePoints, hashedNodePoint, new HashNodePointComparer<T>());
            if (index < 0)
            {
                index = -index;
                if (index >= _hashedNodePoints.Length)
                {
                    index = 0;
                }
            }
            
            if (_hashedNodePoints.Length > 0 && index >= 0 && index < _hashedNodePoints.Length)
            {
                return _hashedNodePoints[index].NodeIdentifier;   
            }

            return default!;
        }

        public override IHashRing<T> Copy()
        {
            throw new NotImplementedException();
        }
        
        private HashedNodePoint<T>[] Less()
        {
            var less = new HashedNodePoint<T>[_hashedNodePoints.Length - PointsPerNode];
            return less;
        }
        
        private int MoreStartingAt()
        {
            var previous = _hashedNodePoints;
            _hashedNodePoints = new HashedNodePoint<T>[previous.Length + PointsPerNode];
            Array.Copy(previous, 0, _hashedNodePoints, 0, previous.Length);
            return previous.Length;
        }
    }
}