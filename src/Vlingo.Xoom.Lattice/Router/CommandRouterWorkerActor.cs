// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Lattice.Model;

namespace Vlingo.Xoom.Lattice.Router;

public class CommandRouterWorkerActor : Actor, ICommandRouter
{
    private readonly Stage _stage;
        
    public CommandRouterWorkerActor() => _stage = Stage;

    public void Route<TProtocol, TCommand, TAnswer>(RoutableCommand<TProtocol, TCommand, TAnswer> command) where TCommand : Command => 
        command.HandleWithin(_stage);

    public CommandRouterType CommandRouterType => CommandRouterType.NotDefined;
}