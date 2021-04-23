// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;
using Vlingo.Lattice.Exchange;
using Vlingo.Xoom.Actors.TestKit;

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

        public ExchangeReceivers()
        {
            Access = AccessSafely.AfterCompleting(5);
            
            DoStepOneReceiver = new DoStepOneReceiver(Access);
            DoStepTwoReceiver = new DoStepTwoReceiver(Access);
            DoStepThreeReceiver = new DoStepThreeReceiver(Access);
            DoStepFourReceiver = new DoStepFourReceiver(Access);
            DoStepFiveReceiver = new DoStepFiveReceiver(Access);

            var stepCount = new AtomicInteger(0);
            
            Access.WritingWith<int>("stepCount", delta => stepCount.IncrementAndGet())
                .ReadingWith("stepCount", () => stepCount.Get());
        }

        public void SetProcess(IFiveStepProcess process)
        {
            DoStepOneReceiver.SetProcess(process);
            DoStepTwoReceiver.SetProcess(process);
            DoStepThreeReceiver.SetProcess(process);
            DoStepFourReceiver.SetProcess(process);
            DoStepFiveReceiver.SetProcess(process);
        }
    }
    
    public class DoStepOneReceiver : DefaultExchangeReceiver<DoStepOne>
    {
        private IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepOneReceiver(AccessSafely access) => _access = access;

        public override void Receive(DoStepOne message)
        {
            _process.StepOneHappened();
            _access.WriteUsing("stepCount", 1);
        }

        public void SetProcess(IFiveStepProcess process) => _process = process;
    }
    
    public class DoStepTwoReceiver : DefaultExchangeReceiver<DoStepTwo>
    {
        private IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepTwoReceiver(AccessSafely access) => _access = access;

        public override void Receive(DoStepTwo message)
        {
            _process.StepTwoHappened();
            _access.WriteUsing("stepCount", 1);
        }
        
        public void SetProcess(IFiveStepProcess process) => _process = process;
    }
    
    public class DoStepThreeReceiver : DefaultExchangeReceiver<DoStepThree>
    {
        private IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepThreeReceiver(AccessSafely access) => _access = access;

        public override void Receive(DoStepThree message)
        {
            _process.StepThreeHappened();
            _access.WriteUsing("stepCount", 1);
        }
        
        public void SetProcess(IFiveStepProcess process) => _process = process;
    }
    
    public class DoStepFourReceiver : DefaultExchangeReceiver<DoStepFour>
    {
        private IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepFourReceiver(AccessSafely access) => _access = access;

        public override void Receive(DoStepFour message)
        {
            _process.StepFourHappened();
            _access.WriteUsing("stepCount", 1);
        }
        
        public void SetProcess(IFiveStepProcess process) => _process = process;
    }
    
    public class DoStepFiveReceiver : DefaultExchangeReceiver<DoStepFive>
    {
        private IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepFiveReceiver(AccessSafely access) => _access = access;

        public override void Receive(DoStepFive message)
        {
            _process.StepFiveHappened();
            _access.WriteUsing("stepCount", 1);
        }
        
        public void SetProcess(IFiveStepProcess process) => _process = process;
    }
}