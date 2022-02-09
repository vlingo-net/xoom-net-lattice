// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Symbio.Store.Object;

namespace Vlingo.Xoom.Lattice.Tests.Model.Process
{
    public class StepCountState : StateObject
    {
        private int _stepCount;

        public StepCountState(int stepCount) => _stepCount = stepCount;

        public StepCountState() => _stepCount = 0;

        public void CountStep() => ++_stepCount;

        public int StepCount() => _stepCount;
    }
}