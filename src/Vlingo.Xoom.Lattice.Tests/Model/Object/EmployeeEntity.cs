// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model.Object;

namespace Vlingo.Xoom.Lattice.Tests.Model.Object;

public class EmployeeEntity : ObjectEntity<EmployeeState>, IEmployee
{
    private EmployeeState _employee;
        
    public EmployeeEntity(string id) : base(id) => _employee = new EmployeeState(long.Parse(id), id, 0);

    protected override EmployeeState StateObject => _employee;

    protected override void OnStateObject(EmployeeState stateObject) => _employee = stateObject;

    public ICompletes<EmployeeState> Current() => Completes().With(_employee);

    public ICompletes<EmployeeState> Adjust(int salary)
        => Apply(_employee.With(salary), new EmployeeSalaryAdjusted(), () => _employee);

    public ICompletes<EmployeeState> Hire(int salary)
        => Apply(_employee.With(salary), new EmployeeHired(), () => _employee);
}