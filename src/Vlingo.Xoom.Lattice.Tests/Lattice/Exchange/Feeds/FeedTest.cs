// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Tests.Lattice.Model;
using Vlingo.Tests.Lattice.Model.Sourcing;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Exchange.Feeds;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Journal;
using Vlingo.Xoom.Symbio.Store.Journal.InMemory;
using Xunit;
using Xunit.Abstractions;
using Result = Vlingo.Xoom.Symbio.Store.Result;

namespace Vlingo.Tests.Lattice.Exchange.Feeds
{
    public class FeedTest : IDisposable
    {
        private readonly MockFeedConsumer _consumer;
        private readonly MockJournalDispatcher _dispatcher;
        private readonly IJournalReader _entryReader;
        private readonly IAppendResultInterest _interest;
        private readonly IJournal<string> _journal;
        private readonly World _world;

        [Fact]
        public void TestThatDefaultFeedIsCreated()
        {
            var feed = Feed.DefaultFeedWith(_world.Stage, "test",
                typeof(TextEntryReaderFeeder), _entryReader);

            Assert.NotNull(feed);
        }

        [Fact]
        public void TestThatDefaultFeedReadsFirstFeedItemIncomplete()
        {
            var feed = Feed.DefaultFeedWith(_world.Stage, "test", typeof(TextEntryReaderFeeder), _entryReader);

            var dispatcherAccess = _dispatcher.AfterCompleting(3);

            _journal.Append("stream-1", 1, new TestEvents.Event1(), _interest, null);
            _journal.Append("stream-2", 1, new TestEvents.Event2(), _interest, null);
            _journal.Append("stream-3", 1, new TestEvents.Event3(), _interest, null);

            var count = dispatcherAccess.ReadFrom<int>("entriesCount");

            Assert.Equal(3, count);

            var consumerAccess = _consumer.AfterCompleting(1);

            feed.Feeder.FeedItemTo(FeedItemId.With(1), _consumer);
            
            var feedItems = consumerAccess.ReadFrom<Dictionary<long, FeedItem>>("feedItems");

            Assert.Single(feedItems);
            Assert.False(feedItems[1L].Archived);
        }

        [Fact]
        public void TestThatDefaultFeedReadsFirstFeedItemArchived()
        {
            var feed = Feed.DefaultFeedWith(_world.Stage, "test", typeof(TextEntryReaderFeeder), _entryReader);

            var dispatcherAccess = _dispatcher.AfterCompleting(feed.MessagesPerFeedItem);

            for (var idx = 0; idx < feed.MessagesPerFeedItem; ++idx)
            {
                _journal.Append("stream-" + idx, 1, new TestEvents.Event1(), _interest, null);
            }

            var count = dispatcherAccess.ReadFrom<int>("entriesCount");

            Assert.Equal(feed.MessagesPerFeedItem, count);

            var consumerAccess = _consumer.AfterCompleting(1);

            feed.Feeder.FeedItemTo(FeedItemId.With(1), _consumer);
            
            var feedItems = consumerAccess.ReadFrom<Dictionary<long, FeedItem>>("feedItems");

            Assert.Single(feedItems);
            Assert.Equal(feed.MessagesPerFeedItem, feedItems[1L].Messages.Count());
            Assert.True(feedItems[1L].Archived);
        }

        [Fact]
        public void TestThatDefaultFeedReadsFirstFeedItemArchivedSecondFeedItemIncomplete()
        {
            var feed = Feed.DefaultFeedWith(_world.Stage, "test", typeof(TextEntryReaderFeeder), _entryReader);

            var extra = 3;
            var entries = feed.MessagesPerFeedItem + extra;

            var dispatcherAccess = _dispatcher.AfterCompleting(entries);

            for (var idx = 0; idx < entries; ++idx)
            {
                _journal.Append("stream-" + idx, 1, new TestEvents.Event1(), _interest, null);
            }

            var count = dispatcherAccess.ReadFrom<int>("entriesCount");

            Assert.Equal(entries, count);

            var consumerAccess = _consumer.AfterCompleting(2);

            feed.Feeder.FeedItemTo(FeedItemId.With(1), _consumer);
            feed.Feeder.FeedItemTo(FeedItemId.With(2), _consumer);

            var feedItems = consumerAccess.ReadFrom<Dictionary<long, FeedItem>>("feedItems");

            Assert.Equal(2, feedItems.Count);
            Assert.Equal(feed.MessagesPerFeedItem, feedItems[1L].Messages.Count());
            Assert.True(feedItems[1L].Archived);
            Assert.Equal(extra, feedItems[2L].Messages.Count());
            Assert.False(feedItems[2L].Archived);
        }

        [Fact]
        public void TestThatDefaultFeedReadsThreeItems()
        {
            var feed = Feed.DefaultFeedWith(_world.Stage, "test", typeof(TextEntryReaderFeeder), _entryReader);

            var extra = feed.MessagesPerFeedItem / 2;
            var entries = feed.MessagesPerFeedItem * 2 + extra;

            var dispatcherAccess = _dispatcher.AfterCompleting(entries);

            for (var idx = 0; idx < entries; ++idx)
            {
                _journal.Append("stream-" + idx, 1, new TestEvents.Event1(), _interest, null);
            }

            var count = dispatcherAccess.ReadFrom<int>("entriesCount");

            Assert.Equal(entries, count);

            var consumerAccess = _consumer.AfterCompleting(3);

            feed.Feeder.FeedItemTo(FeedItemId.With(1), _consumer);
            feed.Feeder.FeedItemTo(FeedItemId.With(2), _consumer);
            feed.Feeder.FeedItemTo(FeedItemId.With(3), _consumer);

            var feedItems = consumerAccess.ReadFrom<Dictionary<long, FeedItem>>("feedItems");

            Assert.Equal(3, feedItems.Count);
            Assert.Equal(feed.MessagesPerFeedItem, feedItems[1L].Messages.Count());
            Assert.True(feedItems[1L].Archived);
            Assert.Equal(feed.MessagesPerFeedItem, feedItems[2L].Messages.Count());
            Assert.True(feedItems[2L].Archived);
            Assert.Equal(extra, feedItems[3L].Messages.Count());
            Assert.False(feedItems[3L].Archived);
        }

        public FeedTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);

            _world = World.StartWithDefaults("feed-test");

            _dispatcher = new MockJournalDispatcher();

            _journal = _world.ActorFor<IJournal<string>>(() => new InMemoryJournalActor<string>(_dispatcher));

            _entryReader = _journal.JournalReader("feed-test-reader").Await<IJournalReader>();

            _consumer = new MockFeedConsumer();

            _interest = NoOpInterest();
        }

        public void Dispose()
        {
            _entryReader.Close();
            _world.Terminate();
        }

        private IAppendResultInterest NoOpInterest() => new NoOpAppendResultInterest();
    }

    internal class NoOpAppendResultInterest : IAppendResultInterest
    {
        public void AppendResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome,
            string streamName, int streamVersion, TSource source,
            Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
        {
        }

        public void AppendResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome,
            string streamName, int streamVersion, TSource source,
            Metadata metadata, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
        {
        }

        public void AppendAllResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome,
            string streamName, int streamVersion,
            IEnumerable<TSource> sources, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
        {
        }

        public void AppendAllResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome,
            string streamName, int streamVersion,
            IEnumerable<TSource> sources, Metadata metadata, Optional<TSnapshotState> snapshot, object @object)
            where TSource : ISource
        {
        }
    }
}