// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Lattice
{
    public static class GuidExtensions
    {
        public static long ToLeastSignificantBits(this Guid id)
        {
            var bytes = id.ToByteArray();
            var boolArray = new bool[bytes.Length];
            for(var i = 0; i < bytes.Length; i++)
            {
                boolArray[i] = GetBit(bytes[i]);
            }

            return BitConverter.ToInt64(bytes, 0);
        }
        
        private static bool GetBit(byte b) => (b & 1) != 0;
    }
}