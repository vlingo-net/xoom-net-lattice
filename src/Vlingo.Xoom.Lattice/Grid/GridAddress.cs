// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Grid
{
    [Serializable]
    public class GridAddress : GuidAddress
    {
        public override bool IsDistributable => true;
        
        internal GridAddress(Guid reservedId) : this(reservedId, null, false)
        {
        }

        internal GridAddress(Guid reservedId, string? name) : this(reservedId, name, false)
        {
        }

        internal GridAddress(Guid reservedId, string? name, bool prefixName): base(reservedId, name, prefixName)
        {
        }
    }
}