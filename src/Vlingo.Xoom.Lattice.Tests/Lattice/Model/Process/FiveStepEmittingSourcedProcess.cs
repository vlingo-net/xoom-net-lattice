// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model.Process;
using Vlingo.Xoom.Symbio.Store.Object;

namespace Vlingo.Tests.Lattice.Model.Process
{
    public class FiveStepEmittingSourcedProcess : SourcedProcess<PorcessObjectState>, IFiveStepProcess
    {
        private int _stepCount;

        public override string ProcessId => StreamName;

        public FiveStepEmittingSourcedProcess() => RegisterConsumer<ProcessMessage>(ApplyProcessMessage);

        public ICompletes<int> QueryStepCount() => Completes().With(_stepCount);

        public void StepOneHappened() => Process(new DoStepTwo());

        public void StepTwoHappened() => Process(new DoStepThree());

        public void StepThreeHappened() => Process(new DoStepFour());

        public void StepFourHappened() => Process(new DoStepFive());

        public void StepFiveHappened() => ++_stepCount;

        private void ApplyProcessMessage(ProcessMessage message) => ++_stepCount;
    }

    public class PorcessObjectState : StateObject
    {
    }
}