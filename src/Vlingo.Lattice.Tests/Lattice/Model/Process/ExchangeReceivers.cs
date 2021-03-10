// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Vlingo.Lattice.Exchange;

namespace Vlingo.Tests.Lattice.Model.Process
{
    public class ExchangeReceivers
    {
        public AccessSafely Access { get; }

        public DoStepOneReceiver DoStepOneReceiver { get; }
        public DoStepTwoReceiver DoStepTwoReceiver { get; }
        public DoStepThreeReceiver DoStepThreeReceiver { get; }
        public DoStepFourReceiver DoStepFourReceiver { get; }
        public DoStepFiveReceiver DoStepFiveReceiver{ get; }
        
        private readonly AtomicInteger _stepCount;
        private IFiveStepProcess _process;

        public ExchangeReceivers()
        {
            DoStepOneReceiver = new DoStepOneReceiver(_process, Access);
            DoStepTwoReceiver = new DoStepTwoReceiver(_process, Access);
            DoStepThreeReceiver = new DoStepThreeReceiver(_process, Access);
            DoStepFourReceiver = new DoStepFourReceiver(_process, Access);
            DoStepFiveReceiver = new DoStepFiveReceiver(_process, Access);

            _stepCount = new AtomicInteger(0);

            Access = AccessSafely.AfterCompleting(5);
            Access.WritingWith<int>("stepCount", delta => _stepCount.IncrementAndGet())
                .ReadingWith("stepCount", () => _stepCount.Get());
        }
        
        public void Process(IFiveStepProcess process) => _process = process;
    }
    
    public class DoStepOneReceiver : IExchangeReceiver<DoStepOne>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepOneReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public void Receive(DoStepOne message)
        {
            _process.StepOneHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
    
    public class DoStepTwoReceiver : IExchangeReceiver<DoStepTwo>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepTwoReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public void Receive(DoStepTwo message)
        {
            _process.StepOneHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
    
    public class DoStepThreeReceiver : IExchangeReceiver<DoStepThree>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepThreeReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public void Receive(DoStepThree message)
        {
            _process.StepOneHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
    
    public class DoStepFourReceiver : IExchangeReceiver<DoStepFour>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepFourReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public void Receive(DoStepFour message)
        {
            _process.StepOneHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
    
    public class DoStepFiveReceiver : IExchangeReceiver<DoStepFive>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepFiveReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public void Receive(DoStepFive message)
        {
            _process.StepOneHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
}