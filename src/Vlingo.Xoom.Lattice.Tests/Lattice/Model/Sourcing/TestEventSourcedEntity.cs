// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model.Sourcing;

namespace Vlingo.Tests.Lattice.Model.Sourcing
{
    public class TestEventSourcedEntity : EventSourced, IEntity
    {
        private readonly Result _result;

        public TestEventSourcedEntity(Result result) : base("TestEvent123")
        {
            _result = result;
            RegisterConsumer<Test1Happened>(Applied1);
            RegisterConsumer<Test2Happened>(Applied2);
            RegisterConsumer<Test3Happened>(Applied3);
        }

        public void DoTest1() => Apply(new Test1Happened());

        public void DoTest2() => Apply(new Test2Happened());

        public ICompletes<string> DoTest3() => Apply(new Test3Happened(), () => "hello");

        public async Task<string> DoTest4() => await Apply(new Test3Happened(), () => "hello task");

        private void Applied1(Test1Happened @event)
        {
            _result.Access().WriteUsing("tested1", true);
            _result.Access().WriteUsing("applied", @event);
        }

        private void Applied2(Test2Happened @event)
        {
            _result.Access().WriteUsing("tested2", true);
            _result.Access().WriteUsing("applied", @event);
        }
        
        private void Applied3(Test3Happened @event)
        {
            _result.Access().WriteUsing("tested3", true);
            _result.Access().WriteUsing("applied", @event);
        }
    }
}