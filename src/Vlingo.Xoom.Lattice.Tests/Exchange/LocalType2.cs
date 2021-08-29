// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Tests.Exchange
{
    public class LocalType2
    {
        public string Attribute1 { get; }
        public int Attribute2 { get; }
        
        public LocalType2(string value1, int value2)
        {
            Attribute1 = value1;
            Attribute2 = value2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var otherLocalType = (LocalType2) obj;

            return Attribute1.Equals(otherLocalType.Attribute1) && Attribute2 == otherLocalType.Attribute2;
        }

        protected bool Equals(LocalType2 other) => Attribute1 == other.Attribute1 && Attribute2 == other.Attribute2;

        public override int GetHashCode() => HashCode.Combine(Attribute1, Attribute2);

        public override string ToString() => $"LocalType2[attribute1={Attribute1} attribute2={Attribute2}]";
    }
}