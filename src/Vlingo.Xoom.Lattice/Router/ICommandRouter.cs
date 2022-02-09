// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Lattice.Model;

namespace Vlingo.Xoom.Lattice.Router
{
    public interface ICommandRouter
    {
        void Route<TProtocol, TCommand, TAnswer>(RoutableCommand<TProtocol, TCommand, TAnswer> command) where TCommand : Command;

        CommandRouterType CommandRouterType { get; }
    }

    public enum CommandRouterType
    {
        LoadBalancing,
        Partitioning,
        RoundRobin,
        NotDefined
    }
    
    public static class CommandRouter
    {
        public static ICommandRouter Of(Stage stage, CommandRouterType type, int totalRoutees)
        {
            switch (type)
            {
                case CommandRouterType.LoadBalancing:
                    return stage.ActorFor<ICommandRouter>(() => new LoadBalancingCommandRouter(totalRoutees));
                case CommandRouterType.Partitioning:
                    return stage.ActorFor<ICommandRouter>(() => new PartitioningCommandRouter(totalRoutees));
                case CommandRouterType.RoundRobin:
                    return stage.ActorFor<ICommandRouter>(() => new RoundRobinCommandRouter(totalRoutees));
            }
            throw new ArgumentException($"The command router type is not mapped: {type}");
        }
    }
}