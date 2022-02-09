// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Model.Projection
{
    public class TextProjectable : AbstractProjectable
    {
        public TextProjectable(IState state, IEnumerable<IEntry> entries, string projectionId) : base(state, entries, projectionId)
        {
        }

        public override string DataAsText() => TextState.Data;
    }
}