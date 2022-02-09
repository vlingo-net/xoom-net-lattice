// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model.Process;

namespace Vlingo.Xoom.Lattice.Tests.Model.Process
{
    public class FiveStepSendingSourcedProcess : SourcedProcess<PorcessObjectState>, IFiveStepProcess
    {
        private int _stepCount;

        public FiveStepSendingSourcedProcess() : base("12345")
        {
        }

        public override string ProcessId => StreamName;
        
        public ICompletes<int> QueryStepCount() => Completes().With(_stepCount);

        public void StepOneHappened()
        {
            ++_stepCount;
            Send(new DoStepTwo());
        }

        public void StepTwoHappened()
        {
            ++_stepCount;
            Send(new DoStepThree());
        }

        public void StepThreeHappened()
        {
            ++_stepCount;
            Send(new DoStepFour());
        }

        public void StepFourHappened()
        {
            ++_stepCount;
            Send(new DoStepFive());
        }

        public void StepFiveHappened() => ++_stepCount;
    }
}