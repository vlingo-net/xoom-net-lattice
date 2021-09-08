// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Lattice.Model;

namespace Vlingo.Xoom.Lattice.Router
{
    /// <summary>
    /// The <see cref="ICommandRouter"/> implementation for load-balancing on the <see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/>.
    /// </summary>
    public class LoadBalancingCommandRouter : SmallestMailboxRouter<ICommandRouter>, ICommandRouter
    {
        public LoadBalancingCommandRouter(int totalRoutees) :
            base(new RouterSpecification<ICommandRouter>(totalRoutees, Definition.Has(typeof(CommandRouterWorkerActor), Definition.NoParameters)))
        {
        }

        public void Route<TProtocol, TCommand, TAnswer>(RoutableCommand<TProtocol, TCommand, TAnswer> command) where TCommand : Command => 
            DispatchCommand((router, cmd) => router.Route(cmd), command);

        public CommandRouterType CommandRouterType => CommandRouterType.LoadBalancing;
    }
}