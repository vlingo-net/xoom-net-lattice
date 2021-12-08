// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Grid.Hashring;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Lattice.Tests.Grid.Hashring
{
    public class HashRingTest
    {
        private readonly ITestOutputHelper _output;
        private const int Elements = 1_000_000;
        private const int Nodes = 5;
        private const int PointsPerNode = 100;

        private const int Excluded = 0;
        private const int Included = 500;

        [Fact]
        public void TestMd5ListHashRing()
        {
            _output.WriteLine("\ntestMD5ListHashRing()\n============================");

            var ring = new MD5ArrayListRing<string>(PointsPerNode, (i, id) => new HashedNodePointMock(i, id));

            IncludeNodes(ring);
            var elementsPerNode = new int[Nodes];

            PopulateElements(ring, elementsPerNode);

            Dump(elementsPerNode);
        }
        
        [Fact]
        public void TestMd5ArrayHashRing()
        {
            _output.WriteLine("\ntestMD5ArrayHashRing()\n============================");

            var ring = new MD5ArrayHashRing<string>(PointsPerNode, (i, id) => new HashedNodePointMock(i, id));

            IncludeNodes(ring);
            var elementsPerNode = new int[Nodes];

            PopulateElements(ring, elementsPerNode);

            Dump(elementsPerNode);
        }
        
        [Fact]
        public void TestMurmurArrayHashRing()
        {
            // The consistent winner: MurmurArrayHashRing

            _output.WriteLine("\ntestMurmurArrayHashRing()\n============================");
            
            var ring = new MurmurArrayHashRing<string>(PointsPerNode, (i, id) => new HashedNodePointMock(i, id));

            IncludeNodes(ring);
            var elementsPerNode = new int[Nodes];

            PopulateElements(ring, elementsPerNode);

            Dump(elementsPerNode);

            Assert.Equal(0, Excluded);
            Assert.Equal(Nodes * PointsPerNode, Included);
        }
        
        public HashRingTest(ITestOutputHelper output)
        {
            _output = output;
            var converter = new Converter(output);
            Console.SetOut(converter);
            
        }
        
        private void Dump(int[] elementsPerNode)
        {
            for (var idx = 0; idx < Nodes; ++idx)
            {
                _output.WriteLine("node{0}={1}", idx, elementsPerNode[idx]);
            }
        }

        private void IncludeNodes<T>(IHashRing<T> ring)
        {
            for (var idx = 0; idx < Nodes; ++idx)
            {
                ring.IncludeNode((T)(object)("node" + idx));
            }
        }

        private void PopulateElements<T>(IHashRing<T> ring, int[] elementsPerNode)
        {
            var startTime = DateExtensions.GetCurrentMillis();

            for (var idx = 0; idx < Elements; ++idx)
            {
                var node = ring.NodeOf(Guid.NewGuid().ToString());
                var nodeIndex = node.ToString()![4] - '0';
                ++elementsPerNode[nodeIndex];
            }

            _output.WriteLine("Time in ms: {0}", DateExtensions.GetCurrentMillis() - startTime);
        }
    }
}