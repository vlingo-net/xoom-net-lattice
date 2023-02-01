// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model.Sourcing;

namespace Vlingo.Xoom.Lattice.Tests.Model.Sourcing;

public class TestCommandSourcedEntity : CommandSourced, IEntity
{
    private readonly Result _result;

    public TestCommandSourcedEntity(Result result) : base("TestCommand123")
    {
        _result = result;
        RegisterConsumer<DoCommand1>(Applied1);
        RegisterConsumer<DoCommand2>(Applied2);
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