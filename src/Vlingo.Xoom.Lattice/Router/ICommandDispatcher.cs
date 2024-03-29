// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Lattice.Model;

namespace Vlingo.Xoom.Lattice.Router;

public interface ICommandDispatcher<in TProtocol, in TCommand, in TAnswer> where TCommand : Command
{
    void Accept(TProtocol protocol, TCommand command, TAnswer answer);
}