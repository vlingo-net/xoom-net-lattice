// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Symbio.Store.Object;

namespace Vlingo.Tests.Lattice.Model.Object
{
    public class EmployeeState : StateObject, IComparable<EmployeeState>
    {
        public EmployeeState(long id, string number, int salary) : base(id)
        {
            Number = number;
            Salary = salary;
        }
        
        public EmployeeState With(string number) => new EmployeeState(PersistenceId, number, Salary);

        public EmployeeState With(int salary) => new EmployeeState(PersistenceId, Number, salary);
        
        public int Salary { get; }
        
        public string Number { get; }
        
        public int CompareTo(EmployeeState other) => PersistenceId.CompareTo(other.PersistenceId);

        public override int GetHashCode() => 31 * Number.GetHashCode() * Salary;

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            else if (this == obj)
            {
                return true;
            }

            var otherPerson = (EmployeeState) obj;

            return PersistenceId == otherPerson.PersistenceId;
        }

        public override string ToString() => $"EmployeeState[persistenceId={PersistenceId} number={Number} salary={Salary}]";
    }
}