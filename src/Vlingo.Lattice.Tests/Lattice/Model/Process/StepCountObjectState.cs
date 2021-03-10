// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Symbio.Store.Object;

namespace Vlingo.Tests.Lattice.Model.Process
{
    public class StepCountObjectState : StateObject, IComparable<StepCountObjectState>
    {
        public int StepCount { get; private set; } = 0;

        public StepCountObjectState(long id) : base(id)
        {
        }
        
        public void CountStep() => ++StepCount;

        public int CompareTo(StepCountObjectState other) => PersistenceId.CompareTo(other.PersistenceId);
    }
}