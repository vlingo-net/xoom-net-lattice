// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model.Process;

namespace Vlingo.Tests.Lattice.Model.Process
{
    public class FiveStepEmittingObjectProcess : ObjectProcess<StepCountObjectState>, IFiveStepProcess
    {
        private static readonly AtomicLong IdGenerator = new AtomicLong(0);
        private readonly Chronicle<StepCountObjectState> _chronicle;
        private StepCountObjectState _state;

        public FiveStepEmittingObjectProcess() : this(IdGenerator.IncrementAndGet())
        {
        }
        
        public FiveStepEmittingObjectProcess(long id) : base(id.ToString())
        {
            _state = new StepCountObjectState(id);
            _chronicle = new Chronicle<StepCountObjectState>(_state);
        }

        protected override StepCountObjectState StateObject => _state;
        
        protected override void OnStateObject(StepCountObjectState stateObject)
        {
            _state = stateObject;
            _chronicle.TransitionTo(stateObject);
        }

        public override Chronicle<StepCountObjectState> Chronicle => _chronicle;

        public override string ProcessId => Id;
        
        public ICompletes<int> QueryStepCount() => Completes().With(_state.StepCount);

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