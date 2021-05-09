// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model;

namespace Vlingo.Tests.Lattice.Model.Process
{
    public interface IFiveStepProcess
    {
        ICompletes<int> QueryStepCount();
        void StepOneHappened();
        void StepTwoHappened();
        void StepThreeHappened();
        void StepFourHappened();
        void StepFiveHappened();
    }

    public class DoStepOne : Command
    {
    }

    public class DoStepTwo : Command
    {
    }

    public class DoStepThree : Command
    {
    }

    public class DoStepFour : Command
    {
    }

    public class DoStepFive : Command
    {
    }
}