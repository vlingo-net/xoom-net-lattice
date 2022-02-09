// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    public class Lease : Period
    {
        public new static Lease Forever = Of(DateTime.MaxValue.GetCurrentSeconds());
        public new static Lease None = Of(0);

        public new static Lease Of(TimeSpan duration) => new Lease(duration);

        public new static Lease Of(long period) => new Lease(TimeSpan.FromSeconds(period));

        protected Lease(TimeSpan duration) : base(duration)
        {
        }
    }
}