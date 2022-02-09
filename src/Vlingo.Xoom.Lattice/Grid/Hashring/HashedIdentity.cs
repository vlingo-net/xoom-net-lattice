// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Grid.Hashring
{
    public class HashedIdentity : IComparable<int>
    {
        public int Hash { get; }

        public HashedIdentity(int hash) => Hash = hash;

        public int CompareTo(int other) => other - Hash;

        public override string ToString() => $"HashedIdentity[hash={Hash}]";
    }
}