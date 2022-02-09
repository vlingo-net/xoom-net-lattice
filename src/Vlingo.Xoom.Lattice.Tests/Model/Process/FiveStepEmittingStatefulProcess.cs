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
    public class FiveStepEmittingStatefulProcess : StatefulProcess<StepCountState>, IFiveStepProcess
    {
        private readonly Chronicle<StepCountState> _chronicle;
        private StepCountState _state;

        public FiveStepEmittingStatefulProcess() : base("12345")
        {
            _state = new StepCountState();
            _chronicle = new Chronicle<StepCountState>(_state);
        }
        
        protected override void State(StepCountState state) => _state = state;

        public override Chronicle<StepCountState> Chronicle => _chronicle;

        public override string ProcessId => Id;
        
        public ICompletes<int> QueryStepCount() => Completes().With(_state.StepCount());

        public void StepOneHappened()
        {
            _state.CountStep();
            Process(new DoStepTwo());
        }

        public void StepTwoHappened()
        {
            _state.CountStep();
            Process(new DoStepThree());
        }

        public void StepThreeHappened()
        {
            _state.CountStep();
            Process(new DoStepFour());
        }

        public void StepFourHappened()
        {
            _state.CountStep();
            Process(new DoStepFive());
        }

        public void StepFiveHappened() => _state.CountStep();
    }
}