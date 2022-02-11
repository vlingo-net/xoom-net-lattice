// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model;
using Vlingo.Xoom.Lattice.Router;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Lattice.Tests.Router;

public class CommandRouterTest
{
    private readonly IAddress _address;
    private readonly ICompletes<Stuff> _completes;
    private readonly World _world;

    [Fact]
    public void TestThatCommandIsHandled()
    {
        var command = RoutableCommand<ISolver, SolveStuff, Stuff>
            .Speaks()
            .To<SolverActor>()
            .At(_address.IdString)
            .Delivers(SolveStuff.With("123", 21))
            .Answers(_completes)
            .HandledBy(SolverHandler.NewInstance(new Result()));

        command.HandleWithin(_world.Stage);

        var stuff = _completes.Await();

        Assert.Equal(42, stuff.Value);
    }
        
    [Fact]
    public void TestThatPartitioningRouterRoutes()
    {
        var router = CommandRouter.Of(_world.Stage, CommandRouterType.Partitioning, 5);

        var result = new Result();

        var command = RoutableCommand<ISolver, SolveStuff, Stuff>
            .Speaks()
            .To<SolverActor>()
            .At(_address.IdString)
            .Delivers(SolveStuff.With("123", 21))
            .Answers(_completes)
            .HandledBy(SolverHandler.NewInstance(result));

        router.Route(command);

        var stuff = _completes.Await();

        Assert.Equal(42, stuff.Value);
        Assert.Equal(1, result.CountOf(0));
    }
        
    [Fact]
    public void TestThatPartitioningRouterPartitions()
    {
        var totalPartitions = 5;
        var totalTimes = totalPartitions * 2;

        var completes = new ICompletes<Stuff>[totalTimes];

        var router = CommandRouter.Of(_world.Stage, CommandRouterType.Partitioning, totalPartitions);

        var result = new Result();

        var solverHandlers = new SolverHandler[totalPartitions];

        for (var idx = 0; idx < totalPartitions; ++idx)
        {
            solverHandlers[idx] = SolverHandler.NewInstance(result);
        }

        for (var idx = 0; idx < totalTimes; ++idx)
        {
            completes[idx] = Completes.Using<Stuff>(_world.Stage.Scheduler);

            var partitionSolverHandler = solverHandlers[idx % totalPartitions];

            var command = RoutableCommand<ISolver, SolveStuff, Stuff>
                .Speaks()
                .To<SolverActor>()
                .At(_address.IdString)
                .Delivers(SolveStuff.With($"{idx}", 21))
                .Answers(completes[idx])
                .HandledBy(partitionSolverHandler);

            router.Route(command);
        }

        for (var idx = 0; idx < totalTimes; ++idx)
        {
            var stuff = completes[idx].Await();

            Assert.Equal(42, stuff.Value);
        }

        for (var handlerId = 0; handlerId < totalPartitions; ++handlerId)
        {
            Assert.Equal(2, result.CountOf(handlerId));
        }
    }
        
    [Fact]
    public void TestThatRoundRobinRouterRoutesToAll()
    {
        var totalRoutees = 5;
        var totalTimes = totalRoutees * 3;

        var completes = new ICompletes<Stuff>[totalTimes];

        var router = CommandRouter.Of(_world.Stage, CommandRouterType.RoundRobin, totalRoutees);

        var result = new Result();

        var solverHandlers = new SolverHandler[totalRoutees];

        for (var idx = 0; idx < totalRoutees; ++idx)
        {
            solverHandlers[idx] = SolverHandler.NewInstance(result);
        }

        for (var idx = 0; idx < totalTimes; ++idx)
        {
            completes[idx] = Completes.Using<Stuff>(_world.Stage.Scheduler);

            var roundRobinSolverHandler = solverHandlers[idx % totalRoutees];
                
            var command = RoutableCommand<ISolver, SolveStuff, Stuff>
                .Speaks()
                .To<SolverActor>()
                .At(_address.IdString)
                .Delivers(SolveStuff.With($"{idx}", 21))
                .Answers(completes[idx])
                .HandledBy(roundRobinSolverHandler);

            router.Route(command);
        }

        for (var idx = 0; idx < totalTimes; ++idx)
        {
            var stuff = completes[idx].Await();

            Assert.Equal(42, stuff.Value);
        }

        for (var handlerId = 0; handlerId < totalRoutees; ++handlerId)
        {
            Assert.Equal(3, result.CountOf(handlerId));
        }
    }
        
    [Fact]
    public void TestThatLoadBalancingRouterRoutesEvenly()
    {
        var totalRoutees = 2;
        var totalTimes = totalRoutees * 10;
            
        var completes = new ICompletes<Stuff>[totalTimes];

        var router = CommandRouter.Of(_world.Stage, CommandRouterType.LoadBalancing, totalRoutees);

        var result = new Result();

        var solverHandlers = new SolverHandler[totalRoutees];

        for (var idx = 0; idx < totalRoutees; ++idx)
        {
            solverHandlers[idx] = SolverHandler.NewInstance(result);
        }

        for (var idx = 0; idx < totalTimes; ++idx)
        {
            completes[idx] = Completes.Using<Stuff>(_world.Stage.Scheduler);

            var loadBalancingSolverHandler = solverHandlers[idx % totalRoutees];
                
            var command = RoutableCommand<ISolver, SolveStuff, Stuff>
                .Speaks()
                .To<SolverActor>()
                .At(_address.IdString)
                .Delivers(SolveStuff.With($"{idx}", 21))
                .Answers(completes[idx])
                .HandledBy(loadBalancingSolverHandler);

            router.Route(command);
        }

        for (var idx = 0; idx < totalTimes; ++idx)
        {
            var stuff = completes[idx].Await();

            Assert.Equal(42, stuff.Value);
        }

        // NOTE: It is difficult to impossible to predict which of the
        // routees will have routed equal or more commands. Rather than
        // attempt that ensure that all commands have been handled.
        var totalCounts = 0;
        for (var handlerId = 0; handlerId < totalRoutees; ++handlerId)
        {
            totalCounts += result.CountOf(handlerId);
        }
        Assert.Equal(totalTimes, totalCounts);
    }

    public CommandRouterTest(ITestOutputHelper output)
    {
        var converter = new Converter(output);
        Console.SetOut(converter);

        _world = World.StartWithDefaults("test-command-router");
        _address = _world.AddressFactory.Unique();
        _world.Stage.ActorFor<ISolver>(() => new SolverActor(), _address);
        _completes = Completes.Using<Stuff>(_world.Stage.Scheduler);
    }
}

public class SolveStuff : Command
{
    public int Value { get; }

    public override string Id { get; }

    public static SolveStuff With(string id, int value) => new SolveStuff(id, value);

    private SolveStuff(string id, int value)
    {
        Id = id;
        Value = value;
    }
}

public class SolverHandler : ICommandDispatcher<ISolver, SolveStuff, ICompletes<Stuff>>
{
    private readonly int _handlerId;
    private readonly Result _result;

    public static SolverHandler NewInstance(Result result) => new SolverHandler(result);

    private SolverHandler(Result result)
    {
        _result = result;
        _handlerId = result.NextHandlerId();
    }

    public void Accept(ISolver protocol, SolveStuff command, ICompletes<Stuff> answer)
    {
        _result.CountTimes(_handlerId);
        protocol.SolveStuff(command.Value).AndThenConsume(staff => answer.With(staff));
    }
}

public class Result
{
    private readonly AtomicInteger _handlerId = new AtomicInteger(-1);
    private readonly List<int> _times = new List<int>();
    public int NextHandlerId()
    {
        var id = _handlerId.IncrementAndGet();
        _times.Add(0);
        return id;
    }

    public int CountOf(int handlerId) => _times[handlerId];

    public void CountTimes(int handlerId)
    {
        var count = _times[handlerId];
        _times[handlerId] = count + 1;
    }
}