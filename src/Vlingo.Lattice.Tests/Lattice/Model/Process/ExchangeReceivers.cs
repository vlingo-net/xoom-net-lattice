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

        public ExchangeReceivers(IFiveStepProcess process)
        {
            Access = AccessSafely.AfterCompleting(5);
            
            DoStepOneReceiver = new DoStepOneReceiver(process, Access);
            DoStepTwoReceiver = new DoStepTwoReceiver(process, Access);
            DoStepThreeReceiver = new DoStepThreeReceiver(process, Access);
            DoStepFourReceiver = new DoStepFourReceiver(process, Access);
            DoStepFiveReceiver = new DoStepFiveReceiver(process, Access);

            var stepCount = new AtomicInteger(0);
            
            Access.WritingWith<int>("stepCount", delta => stepCount.IncrementAndGet())
                .ReadingWith("stepCount", () => stepCount.Get());
        }
    }
    
    public class DoStepOneReceiver : DefaultExchangeReceiver<DoStepOne>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepOneReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public override void Receive(DoStepOne message)
        {
            _process.StepOneHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
    
    public class DoStepTwoReceiver : DefaultExchangeReceiver<DoStepTwo>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepTwoReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public override void Receive(DoStepTwo message)
        {
            _process.StepTwoHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
    
    public class DoStepThreeReceiver : DefaultExchangeReceiver<DoStepThree>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepThreeReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public override void Receive(DoStepThree message)
        {
            _process.StepThreeHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
    
    public class DoStepFourReceiver : DefaultExchangeReceiver<DoStepFour>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepFourReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public override void Receive(DoStepFour message)
        {
            _process.StepFourHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
    
    public class DoStepFiveReceiver : DefaultExchangeReceiver<DoStepFive>
    {
        private readonly IFiveStepProcess _process;
        private readonly AccessSafely _access;

        public DoStepFiveReceiver(IFiveStepProcess process, AccessSafely access)
        {
            _process = process;
            _access = access;
        }
    
        public override void Receive(DoStepFive message)
        {
            _process.StepFiveHappened();
            _access.WriteUsing("stepCount", 1);
        }
    }
}