// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Tests.Lattice.Model.Object
{
    public interface IEmployee
    {
        ICompletes<EmployeeState> Current();
        ICompletes<EmployeeState> Adjust(int salary);
        ICompletes<EmployeeState> Hire(int salary);
    }
    
    public class EmployeeHired : TestEvents.Event { }
    public class EmployeeSalaryAdjusted : TestEvents.Event { }
}