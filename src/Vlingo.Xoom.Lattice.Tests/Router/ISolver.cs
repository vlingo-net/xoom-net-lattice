// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Tests.Router;

public interface ISolver
{
    ICompletes<Stuff> SolveStuff(int value);
}
    
public class Stuff
{
    public int Value { get; }

    public Stuff(int value) => Value = value;
}