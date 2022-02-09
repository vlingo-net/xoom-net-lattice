// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Lattice.Grid.Hashring;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Grid.Hashring
{
    public abstract class HashRingPropertyTest
    {
        private const int PointsPerNode = 100;

        private static readonly Func<int, string, HashedNodePoint<string>> _factory =
            (hash, node) => new HashedNodePointMock(hash, node);
        
        private static readonly int _sampleSize = 100;
        private static readonly string[] _nodes = {"node0", "node1", "node2"};

        [Fact]
        public void EqualRingsMustAssignToTheSameNodes()
        {
            var ring1 = IncludeAll(Ring(PointsPerNode, _factory), _nodes);
            var ring2 = IncludeAll(Ring(PointsPerNode, _factory), _nodes);

            foreach (var sample in Gen(_sampleSize))
            {
                Assert.Equal(ring1.NodeOf(sample), ring2.NodeOf(sample));
            }
        }

        [Fact(Skip = "Inconsistent implementation")]
        public void ExcludingNodesMustRetainAssignmentsToRemainingNodes()
        {
            var sample = Gen(_sampleSize);

            var ring = IncludeAll(Ring(PointsPerNode, _factory), _nodes);
            var assignments = Assignments(sample, ring);

            var removed = ExcludeAll(ring, _nodes[1]);
            var assignmentsRemoved = Assignments(sample, removed);

            Assert.Superset(new HashSet<Guid>( assignmentsRemoved[_nodes[0]]), new HashSet<Guid>( assignments[_nodes[0]]));
            Assert.Superset(new HashSet<Guid>( assignmentsRemoved[_nodes[2]]), new HashSet<Guid>( assignments[_nodes[2]]));
        }

        [Fact]
        public void EmptyHashRingShouldAssignNull()
        {
            var ring = Ring(PointsPerNode, _factory);
            Assert.Null(ring.NodeOf(Guid.NewGuid()));
        }
        
        protected static List<Guid> Gen(int n)
        {
            return Enumerable.Range(0, n)
                .Select(ignored => Guid.NewGuid())
                .ToList();
        }

        protected abstract IHashRing<string> Ring(int pointsPerNode, Func<int, string, HashedNodePoint<string>> factory);
        
        private static IHashRing<string> IncludeAll(IHashRing<string> ring, params string[] nodes)
        {
            foreach (var node in nodes)
            {
                ring.IncludeNode(node);
            }
            return ring;
        }
        
        private static Dictionary<string, List<Guid>> Assignments(List<Guid> sample, IHashRing<string> ring)
        {
            var map = sample
                .Select(uuid => new Tuple<Guid, string>(uuid, ring.NodeOf(uuid)))
                .GroupBy(tuple => tuple.Item2)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(tuple => tuple.Item1).ToList());
            
            return map;
        }
        
        private static IHashRing<string> ExcludeAll(IHashRing<string> ring, params string[] nodes)
        {
            foreach (var node in nodes)
            {
                ring.ExcludeNode(node);
            }
            return ring;
        }
    }

    internal class HashedNodePointMock : HashedNodePoint<string>
    {
        public HashedNodePointMock(int hash, string nodeIdentifier) : base(hash, nodeIdentifier)
        {
        }

        public override void Excluded()
        {
        }

        public override void Included()
        {
        }

        public override string ToString() => $"HashedNodePoint[hash={Hash} nodeIdentifier={NodeIdentifier}]";
    }
}