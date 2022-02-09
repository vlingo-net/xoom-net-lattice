// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Threading.Tasks;
using Vlingo.Xoom.Lattice.Grid.Hashring;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Grid.Hashring
{
    public class MurmurSortedMapHashRingTest
    {
        [Fact]
        public async Task TestNodeOfRace()
        {
            var ring = new MurmurSortedMapHashRing<string>(100, (i, id) => new HashedNodePointMock(i, id));

            ring.IncludeNode("node1");
            ring.IncludeNode("node2");
            ring.IncludeNode("node3");

            Func<string> call = () => ring.NodeOf("testing");

            var tasks = Enumerable.Range(0, 1000)
                .Select(i =>
                {
                    var t = new Task<string>(call);
                    t.Start();
                    return t;
                })
                .ToList();

            var result = await Task.WhenAll(tasks);

            Assert.Single(result.Distinct());
        }
    }
}