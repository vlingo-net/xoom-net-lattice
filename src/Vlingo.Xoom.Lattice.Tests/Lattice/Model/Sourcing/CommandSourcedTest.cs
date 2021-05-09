// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Lattice.Model.Sourcing;
using Vlingo.Xoom.Symbio.Store.Journal;
using Vlingo.Xoom.Symbio.Store.Journal.InMemory;
using Vlingo.Xoom.Actors;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Tests.Lattice.Model.Sourcing
{
    public class CommandSourcedTest : IDisposable
    {
        private readonly MockJournalDispatcher _dispatcher;
        private readonly IEntity _entity;
        private readonly IJournal<string> _journal;
        private readonly SourcedTypeRegistry _registry;
        private readonly Result _result;
        private readonly World _world;
        
        [Fact]
        public void TestThatCtorEmits()
        {
            var access = _result.AfterCompleting(2);

            _entity.DoTest1();

            Assert.True(access.ReadFrom<bool>("tested1"));
            Assert.Equal(1, access.ReadFrom<int>("appliedCount"));
            var appliedAt0 = access.ReadFrom<int, object>("appliedAt", 0);
            Assert.NotNull(appliedAt0);
            Assert.Equal(typeof(DoCommand1), appliedAt0.GetType());
            Assert.False(access.ReadFrom<bool>("tested2"));
        }

        [Fact]
        public void TestThatEventEmits()
        {
            var access = _result.AfterCompleting(2);

            _entity.DoTest1();

            Assert.True(access.ReadFrom<bool>("tested1"));
            Assert.False(access.ReadFrom<bool>("tested2"));
            Assert.Equal(1, access.ReadFrom<int>("appliedCount"));
            var appliedAt0 = access.ReadFrom<int, object>("appliedAt", 0);
            Assert.NotNull(appliedAt0);
            Assert.Equal(typeof(DoCommand1), appliedAt0.GetType());

            var access2 = _result.AfterCompleting(2);

            _entity.DoTest2();

            Assert.Equal(2, access2.ReadFrom<int>("appliedCount"));
            var appliedAt1 = access2.ReadFrom<int, object>("appliedAt", 1);
            Assert.NotNull(appliedAt1);
            Assert.Equal(typeof(DoCommand2), appliedAt1.GetType());
        }

        public CommandSourcedTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);

            _world = World.StartWithDefaults("test-cs");

            _dispatcher = new MockJournalDispatcher();

            _journal = _world.ActorFor<IJournal<string>>(() => new InMemoryJournalActor<string>(_dispatcher));

            _registry = new SourcedTypeRegistry(_world);
            _registry.Register(Info.RegisterSourced<TestCommandSourcedEntity>(_journal));
            _registry.Info<TestCommandSourcedEntity>()
                ?.RegisterEntryAdapter(new DoCommand1Adapter())
                .RegisterEntryAdapter(new DoCommand2Adapter())
                .RegisterEntryAdapter(new DoCommand3Adapter());

            _result = new Result();
            _entity = _world.ActorFor<IEntity>(() => new TestCommandSourcedEntity(_result));
        }

        public void Dispose() => _world.Terminate();
    }
}