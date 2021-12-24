// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.Serialization;
using Vlingo.Xoom.Lattice.Grid.Spaces;

namespace Vlingo.Xoom.Lattice.Tests.Grid.Spaces
{
    [Serializable]
    public class Key1 : IKey
    {
        public string Id { get; }

        public Key1(string id) => Id = id;

        public int Compare(IKey x, IKey y)
        {
            var k1 = (Key1) x;
            var k2 = (Key1) y;

            return string.Compare(k1?.Id, k2?.Id, StringComparison.Ordinal);
        }

        public bool Matches(IKey other) => Equals(other);

        public override int GetHashCode() => 31 * Id.GetHashCode();

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            return Id.Equals(((Key1) obj).Id);
        }

        public override string ToString() => "Key1[id=" + Id + "]";
    }
}