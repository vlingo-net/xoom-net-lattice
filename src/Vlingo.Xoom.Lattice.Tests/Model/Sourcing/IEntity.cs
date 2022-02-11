// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Tests.Model.Sourcing;

public interface IEntity
{
    void DoTest1();
    void DoTest2();
    ICompletes<string> DoTest3();
    Task<string> DoTest4();
}