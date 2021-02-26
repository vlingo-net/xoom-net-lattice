// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Vlingo.Common;
using Vlingo.Lattice.Model.Sourcing;

namespace Vlingo.Tests.Lattice.Model.Sourcing
{
    public class TestCommandSourcedEntity : CommandSourced, IEntity
    {
        private readonly Action<DoCommand1> _bi1;
        private readonly Action<DoCommand2> _bi2;
        private readonly Result _result;

        public TestCommandSourcedEntity(Result result) : base("TestCommand123")
        {
            _result = result;
            _bi1 = Applied1;
            _bi2 = Applied2;
            RegisterConsumer(_bi1);
            RegisterConsumer(_bi2);
        }

        public void DoTest1() => Apply(new DoCommand1());

        public void DoTest2() => Apply(new DoCommand2());

        public ICompletes<string> DoTest3() => Apply(new DoCommand3(), () => "hello");

        public async Task<string> DoTest4() => await Apply(new DoCommand3(), () => "hello task");

        private void Applied1(DoCommand1 command)
        {
            _result.Access().WriteUsing("tested1", true);
            _result.Access().WriteUsing("applied", command);
        }

        private void Applied2(DoCommand2 command)
        {
            _result.Access().WriteUsing("tested2", true);
            _result.Access().WriteUsing("applied", command);
        }
    }
}