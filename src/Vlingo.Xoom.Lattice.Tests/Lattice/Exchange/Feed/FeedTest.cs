// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Tests.Lattice.Model.Sourcing;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Exchange.Feed;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Journal;
using Vlingo.Xoom.Symbio.Store.Journal.InMemory;
using Xunit;
using Xunit.Abstractions;
using Result = Vlingo.Xoom.Symbio.Store.Result;

namespace Vlingo.Tests.Lattice.Exchange.Feed
{
    public class FeedTest : IDisposable
    {
        private MockFeedConsumer consumer;
        private MockJournalDispatcher dispatcher;
        private IJournalReader entryReader;
        private IAppendResultInterest interest;
        private IJournal<string> journal;
        private World world;
        
        [Fact]
        public void TestThatDefaultFeedIsCreated()
        {
            var feed = Xoom.Lattice.Exchange.Feed.Feed.DefaultFeedWith(world.Stage, "test", typeof(TextEntryReaderFeeder), entryReader);

            Assert.NotNull(feed);
        }
        
        public FeedTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            world = World.StartWithDefaults("feed-test");

            dispatcher = new MockJournalDispatcher();

            journal = world.ActorFor<IJournal<string>>(() => new InMemoryJournalActor<string>(dispatcher));

            entryReader = journal.JournalReader("feed-test-reader").Await<IJournalReader>();

            consumer = new MockFeedConsumer();

            interest = NoOpInterest();
        }
        
        public void Dispose()
        {
            entryReader.Close();
            world.Terminate();
        }

        private IAppendResultInterest NoOpInterest() => new NoOpAppendResultInterest();
    }
    
    internal class NoOpAppendResultInterest : IAppendResultInterest
    {
        public void AppendResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome, string streamName, int streamVersion, TSource source,
            Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
        {
        }

        public void AppendResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome, string streamName, int streamVersion, TSource source,
            Metadata metadata, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
        {
        }

        public void AppendAllResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome, string streamName, int streamVersion,
            IEnumerable<TSource> sources, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
        {
        }

        public void AppendAllResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome, string streamName, int streamVersion,
            IEnumerable<TSource> sources, Metadata metadata, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
        {
        }
    }
}