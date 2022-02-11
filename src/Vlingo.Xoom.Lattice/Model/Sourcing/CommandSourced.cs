// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Model.Sourcing;

/// <summary>
/// A <see cref="Sourced{T}"/> for concrete types of <see cref="Command"/>.
/// </summary>
public abstract class CommandSourced : Sourced<Command>
{
    public CommandSourced(string streamName) : base(streamName)
    {
    }
}