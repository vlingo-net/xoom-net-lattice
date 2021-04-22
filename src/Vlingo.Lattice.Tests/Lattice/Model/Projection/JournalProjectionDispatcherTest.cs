// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Lattice.Model;
using Vlingo.Lattice.Model.Projection;
using Vlingo.Symbio;
using Vlingo.Symbio.Store;
using Vlingo.Symbio.Store.Journal;
using Vlingo.Symbio.Store.Journal.InMemory;
using Xunit;
using Xunit.Abstractions;
using IDispatcher = Vlingo.Symbio.Store.Dispatch.IDispatcher;

namespace Vlingo.Tests.Lattice.Model.Projection
{
    public class JournalProjectionDispatcherTest : IDisposable
    {
        private static string AccessJournal = "journal";
        private static string AccessProjection = "projection";
        private static string StreamName = "A123";
        
        private AccessHolder _accessHolder;
        private readonly IAppendResultInterest _appendInterest;
        private readonly IJournal<string> _journal;
        private readonly World _world;
        
        [Fact]
        public void TestThatOneTwoAllEventsProject()
        {
            _accessHolder.AccessJournalFor(1);
            _accessHolder.AccessProjectionFor(5); // One, Two, and All

            _journal.AppendAll<ISource>(StreamName, 1, new List<ISource> {new OneHappened(), new TwoHappened(), new ThreeHappened()}, _appendInterest, _accessHolder);

            Assert.Equal(1, _accessHolder.AccessJournal.ReadFrom<int>(AccessJournal));
            Assert.True(_accessHolder.AccessProjection.ReadFrom<int>(AccessProjection) >= 5);
        }
        
        [Fact]
        public void TestThatOneEventProject()
        {
            _accessHolder.AccessJournalFor(1);
            _accessHolder.AccessProjectionFor(2); // One and All

            _journal.Append(StreamName, 1, new OneHappened(), _appendInterest, _accessHolder);

            Assert.Equal(1, _accessHolder.AccessJournal.ReadFrom<int>(AccessJournal));
            Assert.Equal(2, _accessHolder.AccessProjection.ReadFrom<int>(AccessProjection));
        }
        
        [Fact]
        public void TestThatTwoEventProject()
        {
            _accessHolder.AccessJournalFor(1);
            _accessHolder.AccessProjectionFor(2); // Two and All

            _journal.Append(StreamName, 1, new TwoHappened(), _appendInterest, _accessHolder);

            Assert.Equal(1, _accessHolder.AccessJournal.ReadFrom<int>(AccessJournal));
            Assert.Equal(2, _accessHolder.AccessProjection.ReadFrom<int>(AccessProjection));
        }
        
        [Fact]
        public void TestThatThreeEventProject()
        {
            _accessHolder.AccessJournalFor(1);
            _accessHolder.AccessProjectionFor(1); // Only All

            _journal.Append(StreamName, 1, new ThreeHappened(), _appendInterest, _accessHolder);

            Assert.Equal(1, _accessHolder.AccessJournal.ReadFrom<int>(AccessJournal));
            Assert.Equal(1, _accessHolder.AccessProjection.ReadFrom<int>(AccessProjection));
        }

        public JournalProjectionDispatcherTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("test-journal-projections");

            _accessHolder = new AccessHolder();

            var descriptions = new List<ProjectToDescription>
            {
                ProjectToDescription<OneHappenedProjectionActor>.With<OneHappened>(() =>
                    new OneHappenedProjectionActor(_accessHolder)),
                ProjectToDescription<TwoHappenedProjectionActor>.With<TwoHappened>(() =>
                    new TwoHappenedProjectionActor(_accessHolder)),
                ProjectToDescription<AllHappenedProjectionActor>
                    .With<OneHappened>(() => new AllHappenedProjectionActor(_accessHolder))
                    .AndWith<TwoHappened>()
                    .AndWith<ThreeHappened>()
            };

            var dispatcherProtocols =
                _world.Stage.ActorFor(
                    new[] { typeof(IDispatcher), typeof(IProjectionDispatcher) },
                    Definition.Has(() => new TextProjectionDispatcherActor(descriptions)));

            var dispatchers = Protocols.Two<IDispatcher, IProjectionDispatcher>(dispatcherProtocols);

            var dispatcher = dispatchers._1;

            _journal = Journal<string>.Using<InMemoryJournalActor<string>>(_world.Stage, dispatcher);

            EntryAdapterProvider.Instance(_world).RegisterAdapter(new OneHappenedAdapter());
            EntryAdapterProvider.Instance(_world).RegisterAdapter(new TwoHappenedAdapter());
            EntryAdapterProvider.Instance(_world).RegisterAdapter(new ThreeHappenedAdapter());

            _appendInterest = _world.Stage.ActorFor<IAppendResultInterest>(() => new JournalAppendResultInterest());
        }
        
        public class AccessHolder
        {
            private readonly AtomicInteger _accessJournalCount = new AtomicInteger(0);
            private readonly AtomicInteger _accessProjectionCount = new AtomicInteger(0);

            public AccessSafely AccessJournal { get; private set; }
            public AccessSafely AccessProjection { get; private set; }

            public void AccessJournalFor(int times)
            {
                AccessJournal = AccessSafely.AfterCompleting(times);

                AccessJournal.WritingWith<int>(JournalProjectionDispatcherTest.AccessJournal, x => _accessJournalCount.IncrementAndGet());
                AccessJournal.ReadingWith(JournalProjectionDispatcherTest.AccessJournal, () => _accessJournalCount.Get());
            }

            public void AccessProjectionFor(int times)
            {
                AccessProjection = AccessSafely.AfterCompleting(times);

                AccessProjection.WritingWith<int>(JournalProjectionDispatcherTest.AccessProjection, x => _accessProjectionCount.IncrementAndGet());
                AccessProjection.ReadingWith(JournalProjectionDispatcherTest.AccessProjection, () => _accessProjectionCount.Get());
            }
        }
        
        public class OneHappened : DomainEvent
        {
        }
        
        public class TwoHappened : DomainEvent
        {
        }
        
        public class ThreeHappened : DomainEvent
        {
        }
        
        public class OneHappenedProjectionActor : Actor, IProjection
        {
            private readonly AccessHolder _accessHolder;

            public OneHappenedProjectionActor(AccessHolder accessHolder) => _accessHolder = accessHolder;

            public void ProjectWith(IProjectable projectable, IProjectionControl control)
            {
                foreach (var entry in projectable.Entries)
                {
                    switch (entry.Typed.Name)
                    {
                        case "OneHappened":
                            _accessHolder.AccessProjection.WriteUsing(AccessProjection, 1);
                            ProjectionControl.ConfirmerFor(projectable, control).Confirm();
                            Logger.Debug("ONE");
                            break;
                    }
                }
                ProjectionControl.ConfirmerFor(projectable, control).Confirm();
            }
        }
        
        public class TwoHappenedProjectionActor : Actor, IProjection
        {
            private readonly AccessHolder _accessHolder;

            public TwoHappenedProjectionActor(AccessHolder accessHolder) => _accessHolder = accessHolder;

            public void ProjectWith(IProjectable projectable, IProjectionControl control)
            {
                foreach (var entry in projectable.Entries)
                {
                    switch (entry.Typed.Name)
                    {
                        case "TwoHappened":
                            _accessHolder.AccessProjection.WriteUsing(AccessProjection, 1);
                            ProjectionControl.ConfirmerFor(projectable, control).Confirm();
                            Logger.Debug("TWO");
                            break;
                    }
                }
                ProjectionControl.ConfirmerFor(projectable, control).Confirm();
            }
        }
        
        public class AllHappenedProjectionActor : Actor, IProjection
        {
            private readonly AccessHolder _accessHolder;
            private int _count;

            public AllHappenedProjectionActor(AccessHolder accessHolder) => _accessHolder = accessHolder;

            public void ProjectWith(IProjectable projectable, IProjectionControl control)
            {
                _count = 0;
                foreach (var entry in projectable.Entries)
                {
                    switch (entry.Typed.Name)
                    {
                        case "OneHappened":
                        case "TwoHappened":
                        case "ThreeHappened":
                            _accessHolder.AccessProjection.WriteUsing(AccessProjection, 1);
                            Logger.Debug($"ALL {++_count}");
                            break;
                    }
                }
                ProjectionControl.ConfirmerFor(projectable, control).Confirm();
            }
        }

        public class JournalAppendResultInterest : Actor, IAppendResultInterest
        {
            public void AppendResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome, string streamName, int streamVersion, TSource source,
                Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
            {
                Logger.Debug("APPENDED Source");
                ((AccessHolder) @object).AccessJournal.WriteUsing(AccessJournal, 1);
            }

            public void AppendResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome, string streamName, int streamVersion, TSource source,
                Metadata metadata, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
            {
                Logger.Debug("APPENDED Source");
                ((AccessHolder) @object).AccessJournal.WriteUsing(AccessJournal, 1);
            }

            public void AppendAllResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome, string streamName, int streamVersion,
                IEnumerable<TSource> sources, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
            {
                Logger.Debug("APPENDED Sources");
                ((AccessHolder) @object).AccessJournal.WriteUsing(AccessJournal, 1);
            }

            public void AppendAllResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome, string streamName, int streamVersion,
                IEnumerable<TSource> sources, Metadata metadata, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
            {
                Logger.Debug("APPENDED Sources");
                ((AccessHolder) @object).AccessJournal.WriteUsing(AccessJournal, 1);
            }
        }

        public class OneHappenedAdapter : EntryAdapter<OneHappened, TextEntry>
        {
            public override OneHappened FromEntry(TextEntry entry) => JsonSerialization.Deserialized<OneHappened>(entry.EntryData);

            public override TextEntry ToEntry(OneHappened source, Metadata metadata)
            {
                var serialization = JsonSerialization.Serialized(source);
                return new TextEntry(typeof(OneHappened), 1, serialization, metadata);
            }

            public override TextEntry ToEntry(OneHappened source, int version, Metadata metadata)
            {
                var serialization = JsonSerialization.Serialized(source);
                return new TextEntry(typeof(OneHappened), version, serialization, metadata);
            }

            public override TextEntry ToEntry(OneHappened source, int version, string id, Metadata metadata)
            {
                var serialization = JsonSerialization.Serialized(source);
                return new TextEntry(id, typeof(OneHappened), version, serialization, metadata);
            }
        }
        
        public class TwoHappenedAdapter : EntryAdapter<TwoHappened, TextEntry>
        {
            public override TwoHappened FromEntry(TextEntry entry) => JsonSerialization.Deserialized<TwoHappened>(entry.EntryData);

            public override TextEntry ToEntry(TwoHappened source, Metadata metadata)
            {
                var serialization = JsonSerialization.Serialized(source);
                return new TextEntry(typeof(TwoHappened), 1, serialization, metadata);
            }

            public override TextEntry ToEntry(TwoHappened source, int version, Metadata metadata)
            {
                var serialization = JsonSerialization.Serialized(source);
                return new TextEntry(typeof(TwoHappened), version, serialization, metadata);
            }

            public override TextEntry ToEntry(TwoHappened source, int version, string id, Metadata metadata)
            {
                var serialization = JsonSerialization.Serialized(source);
                return new TextEntry(id, typeof(TwoHappened), version, serialization, metadata);
            }
        }
        
        public class ThreeHappenedAdapter : EntryAdapter<ThreeHappened, TextEntry>
        {
            public override ThreeHappened FromEntry(TextEntry entry) => JsonSerialization.Deserialized<ThreeHappened>(entry.EntryData);

            public override TextEntry ToEntry(ThreeHappened source, Metadata metadata)
            {
                var serialization = JsonSerialization.Serialized(source);
                return new TextEntry(typeof(ThreeHappened), 1, serialization, metadata);
            }

            public override TextEntry ToEntry(ThreeHappened source, int version, Metadata metadata)
            {
                var serialization = JsonSerialization.Serialized(source);
                return new TextEntry(typeof(ThreeHappened), version, serialization, metadata);
            }

            public override TextEntry ToEntry(ThreeHappened source, int version, string id, Metadata metadata)
            {
                var serialization = JsonSerialization.Serialized(source);
                return new TextEntry(id, typeof(ThreeHappened), version, serialization, metadata);
            }
        }

        public void Dispose()
        {
            _accessHolder = null;
            _world.Terminate();
        }
    }
}