// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Symbio;

namespace Vlingo.Lattice.Model.Projection
{
    public class BinaryProjectable : AbstractProjectable
    {
        public BinaryProjectable(State<byte[]> state, IEnumerable<IEntry> entries, string projectionId) : base(state, entries, projectionId)
        {
        }

        public override byte[] DataAsBytes() => BinaryState.Data;
    }
}