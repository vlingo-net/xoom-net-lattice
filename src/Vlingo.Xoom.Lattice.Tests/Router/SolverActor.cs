// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Tests.Router
{
    public class SolverActor : Actor, ISolver
    {
        public ICompletes<Stuff> SolveStuff(int value) => Completes().With(new Stuff(value * 2));
    }
}