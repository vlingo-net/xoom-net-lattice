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
    /// The <see cref="ICommandRouter"/> implementation for partitioning on the <see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/>.
    /// </summary>
    public class PartitioningCommandRouter : ContentBasedRouter<ICommandRouter>, ICommandRouter
    {
        private Routee<ICommandRouter>? _currentRoutee;
        private readonly int _totalRoutees;
        
        public PartitioningCommandRouter(int totalRoutees) : 
            base(new RouterSpecification<ICommandRouter>(totalRoutees, Definition.Has(typeof(CommandRouterWorkerActor), Definition.NoParameters))) =>
            _totalRoutees = totalRoutees;

        public void Route<TProtocol, TCommand, TAnswer>(RoutableCommand<TProtocol, TCommand, TAnswer> command) where TCommand : Command
        {
            var partition = command.GetHashCode() % _totalRoutees;
            _currentRoutee = RouteeAt(partition);
            DispatchCommand((router, cmd) => router.Route(cmd), command);
        }

        protected override Routing<ICommandRouter> ComputeRouting() => Routing.With(_currentRoutee);

        public CommandRouterType CommandRouterType => CommandRouterType.Partitioning;
    }
}