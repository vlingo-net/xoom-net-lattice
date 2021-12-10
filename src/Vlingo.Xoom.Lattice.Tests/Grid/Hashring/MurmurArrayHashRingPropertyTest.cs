// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Lattice.Grid.Hashring;

namespace Vlingo.Xoom.Lattice.Tests.Grid.Hashring
{
    public class MurmurArrayHashRingPropertyTest : HashRingPropertyTest
    {
        protected override IHashRing<string> Ring(int pointsPerNode, Func<int, string, HashedNodePoint<string>> factory) => 
            new MurmurArrayHashRing<string>(pointsPerNode, factory);
    }
}