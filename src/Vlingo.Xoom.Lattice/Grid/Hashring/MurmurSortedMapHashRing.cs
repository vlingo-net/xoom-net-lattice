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
    public class MurmurSortedMapHashRing<T> : MurmurHashRing<T>
    {
        private readonly List<HashedNodePoint<T>> _hashedNodePoints;
        private static uint DefaultSeed = 31;
        
        public MurmurSortedMapHashRing(int pointsPerNode, Func<int, T, HashedNodePoint<T>> factory) : base(pointsPerNode, factory, DefaultSeed) => 
            _hashedNodePoints = new List<HashedNodePoint<T>>();

        public override void Dump()
        {
            Console.WriteLine($"NODES: {_hashedNodePoints.Count}");
            foreach (var hashedNodePoint in _hashedNodePoints)
            {
                Console.WriteLine($"NODE: {hashedNodePoint}");
            }
        }

        public override IHashRing<T> ExcludeNode(T nodeIdentifier)
        {
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
                    hashedNodePoint.Excluded();
                }
            }

            return this;
        }

        public override IHashRing<T> IncludeNode(T nodeIdentifier)
        {
            for (var element = 0; element < PointsPerNode; ++element)
            {
                var hash = Hashed(nodeIdentifier!.ToString() + element);
                var hashedNodePoint = Factory(hash, nodeIdentifier);
                _hashedNodePoints.Add(hashedNodePoint);
                hashedNodePoint.Included();
            }
            _hashedNodePoints.Sort(new HashNodePointComparer<T>());

            return this;
        }

        public override T NodeOf(object id)
        {
            var hashedNodePoint = HashedNodePointOf(id);
            var index = _hashedNodePoints.BinarySearch(hashedNodePoint, new HashNodePointComparer<T>());
            if (index < 0)
            {
                index = -index;
                if (index >= _hashedNodePoints.Count)
                {
                    index = 0;
                }
            }
            
            if (_hashedNodePoints.Count > 0 && index >= 0 && index < _hashedNodePoints.Count)
            {
                return _hashedNodePoints[index].NodeIdentifier;   
            }

            return default!;
        }

        public override IHashRing<T> Copy()
        {
            throw new NotImplementedException();
        }
    }
}