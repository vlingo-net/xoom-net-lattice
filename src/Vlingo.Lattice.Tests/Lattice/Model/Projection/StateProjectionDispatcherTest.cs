// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Common;
using Vlingo.Lattice.Model.Projection;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Dispatch;
using Vlingo.Symbio.Store.State;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.TestKit;
using Xunit;
using Xunit.Abstractions;
using IDispatcher = Vlingo.Symbio.Store.Dispatch.IDispatcher;

namespace Vlingo.Tests.Lattice.Model.Projection
{
    public class StateProjectionDispatcherTest : ProjectionDispatcherTest
    {
        [Fact]
        public void TestThatTextStateDataProjects()
        {
            var store = Store;

            var interest = new MockResultInterest();

            var projection = new MockProjection();

            var access = projection.AfterCompleting(2);

            ProjectionDispatcher.ProjectTo(projection, new[] { "op1" });
            ProjectionDispatcher.ProjectTo(projection, new[] { "op2" });

            var entity1 = new Entity1("123-1", 1);
            var entity2 = new Entity1("123-2", 2);

            store.Write(entity1.Id, entity1, 1, Metadata.With("value1", "op1"), interest);
            store.Write(entity2.Id, entity2, 1, Metadata.With("value2", "op2"), interest);

            Assert.Equal(2, access.ReadFrom<int>("projections"));
            Assert.Equal("123-1", access.ReadFrom<int, string>("projectionId", 0));
            Assert.Equal("123-2", access.ReadFrom<int, string>("projectionId", 1));
        }
        
        [Fact]
        public void TestThatDescribedProjectionsRegister()
        {
            var description = ProjectToDescription<DescribedProjection>.With(() => new DescribedProjection(), "op1", "op2");
                
            var dispatcher =
                World.ActorFor<IDispatcher>(() => new TextProjectionDispatcherActor(new List<ProjectToDescription> {description}));

            var outcome = new Outcome(2);
            var accessOutcome = outcome.AfterCompleting(2);
            dispatcher.ControlWith(outcome);

            var state = new TextState("123", typeof(object), 1, "blah1", 1, Metadata.With("", "op1"));
            dispatcher.Dispatch(new Dispatchable("123", DateTimeOffset.Now, state, new List<IEntry>()));

            var state2 = new TextState("1235", typeof(object), 1, "blah2", 1, Metadata.With("", "op2"));
            dispatcher.Dispatch(new Dispatchable("1235", DateTimeOffset.Now, state2, new List<IEntry>()));

            Assert.Equal(2, accessOutcome.ReadFrom<int>("count"));
        }
        
        [Fact]
        public void TestThatProjectionsPipeline()
        {
            var store = Store;

            var filterOutcome = new FilterOutcome();
            var filterOutcomeAccess = filterOutcome.AfterCompleting(3);

            var filter1 =
                FilterProjectionDispatcherActor.FilterFor(World, ProjectionDispatcher, new[] {"op-1"}, filterOutcome);

            var filter2 =
                FilterProjectionDispatcherActor.FilterFor(World, filter1, new[] {"op-1"}, filterOutcome);

            FilterProjectionDispatcherActor.FilterFor(World, filter2, new[] {"op-1"}, filterOutcome);

            var entity1 = new Entity1("123-1", 1);

            store.Write(entity1.Id, entity1, 1, Metadata.With("value1", "op-1"), new MockResultInterest());

            Assert.Equal(3, filterOutcomeAccess.ReadFrom<int>("filterCount"));
        }

        public StateProjectionDispatcherTest(ITestOutputHelper output) : base(output)
        {
        }

        protected override Type DispatcherInterfaceType { get; set; } = typeof(IDispatcher);

        protected override Type ProjectionDispatcherType { get; set; } = typeof(TextProjectionDispatcherActor);
        protected override Type StateStoreInterfaceType { get; set; } = typeof(IStateStore);
    }

    public class FilterProjectionDispatcherActor : ProjectionDispatcherActor, IProjection
    {
        private readonly FilterOutcome _outcome;
        
        public static IProjectionDispatcher FilterFor(
            World world,
            IProjectionDispatcher projectionDispatcher,
            string[] becauseOf, 
            FilterOutcome filterOutcome)
        {
            var projectionProtocols =
                world.ActorFor(
                    new [] { typeof(IProjectionDispatcher), typeof(IProjection) },
                    typeof(FilterProjectionDispatcherActor), filterOutcome);

            var projectionFilter = Protocols.Two<IProjectionDispatcher, IProjection>(projectionProtocols);

            projectionDispatcher.ProjectTo(projectionFilter._2, becauseOf);

            return projectionFilter._1;
        }
        
        public FilterProjectionDispatcherActor(FilterOutcome outcome) => _outcome = outcome;

        public override void Dispatch(Dispatchable dispatchable)
        {
        }

        protected override bool RequiresDispatchedConfirmation() => false;

        public void ProjectWith(IProjectable projectable, IProjectionControl control)
        {
            _outcome.Increment();
            control.ConfirmProjected(projectable.ProjectionId);
            Dispatch(projectable.ProjectionId, projectable);
        }

        public override void ProjectTo(IProjection projection, string[] whenMatchingCause) => _outcome.Increment();
    }

    public class FilterOutcome
    {
        private AccessSafely _access = AccessSafely.AfterCompleting(0);
        public AtomicInteger FilterCount { get; }

        public FilterOutcome() => FilterCount = new AtomicInteger(0);

        public void Increment() => _access.WriteUsing("filterCount", 1);

        public AccessSafely AfterCompleting(int times)
        {
            _access = AccessSafely.AfterCompleting(times);
            _access
                .WritingWith<int>("filterCount", increment => FilterCount.AddAndGet(increment))
                .ReadingWith("filterCount", () => FilterCount.Get());

            return _access;
        }
    }
}