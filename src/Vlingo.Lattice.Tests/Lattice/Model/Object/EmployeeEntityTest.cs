// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Lattice.Model.Object;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Object;
using Vlingo.Xoom.Symbio.Store.Object.InMemory;
using Vlingo.Xoom.Actors;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Tests.Lattice.Model.Object
{
    public class EmployeeEntityTest : IDisposable
    {
        private readonly World _world;
        
        [Fact]
        public void TestThatEmployeeIdentifiesModifiesRecovers()
        {
            var employeeNumber = "12345";

            var employee = _world.ActorFor<IEmployee>(() => new EmployeeEntity(employeeNumber));

            var state1 = employee.Hire(50000).Await();
            Assert.True(state1.PersistenceId > 0);
            Assert.Equal(employeeNumber, state1.Number);
            Assert.Equal(50000, state1.Salary);

            var state3 = employee.Adjust(55000).Await();
            Assert.Equal(state1.PersistenceId, state3.PersistenceId);
            Assert.Equal(employeeNumber, state3.Number);
            Assert.Equal(55000, state3.Salary);

            var employeeRecovered = _world.ActorFor<IEmployee>(typeof(EmployeeEntity), employeeNumber);
            var state4 = employeeRecovered.Current().Await();
            Assert.Equal(state3, state4);

            // TODO: test reading event entries
        }
        
        public EmployeeEntityTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("test-object-entity");
            var objectStore = _world.ActorFor<IObjectStore>(typeof(InMemoryObjectStoreActor<string>), new MockDispatcher());

            var registry = new ObjectTypeRegistry(_world);

            // NOTE: The InMemoryObjectStoreActor implementation currently
            // does not use PersistentObjectMapper, and thus the no-op decl.
            var employeeInfo =
                new Info<EmployeeState>(
                    objectStore,
            "HR-Database",
            MapQueryExpression.Using<IEmployee>("find", MapQueryExpression.Map("number", "number")),
            StateObjectMapper.With<IEmployee>(new object(), new object()));

            registry.Register(employeeInfo);
        }

        public void Dispose() => _world.Terminate();
    }
}