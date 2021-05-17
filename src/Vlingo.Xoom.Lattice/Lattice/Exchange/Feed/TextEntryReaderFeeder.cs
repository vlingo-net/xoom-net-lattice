// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;

namespace Vlingo.Xoom.Lattice.Exchange.Feed
{
    /// <summary>
    /// The <see cref="IFeeder"/> serving <see cref="TextEntry"/> instances.
    /// </summary>
    public class TextEntryReaderFeeder : Actor, IFeeder
    {
        private readonly IEntryReader _entryReader;
        private readonly Feed _feed;
        
        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="feed">The <see cref="Feed"/> that is served</param>
        /// <param name="entryReader">The <see cref="IEntryReader"/> from which content is read</param>
        public TextEntryReaderFeeder(Feed feed, IEntryReader entryReader)
        {
            _feed = feed;
            _entryReader = entryReader;
        }
        
        public void FeedItemTo(FeedItemId fromFeedItemId, IFeedConsumer feedConsumer)
        {
            var feedId = fromFeedItemId.ToLong();
            var id = (feedId - 1L) * _feed.MessagesPerFeedItem + 1;

            _entryReader
                .ReadNext(id.ToString(), _feed.MessagesPerFeedItem)
                .AndThen(entries => {
                    var textEntries = entries.ToList();
                    feedConsumer.ConsumeFeedItem(ToFeedItem(fromFeedItemId, textEntries.Cast<TextEntry>().ToList()));
                return textEntries;
            });
        }
        
        /// <summary>
        /// Get a new <see cref="FeedItem"/> from converted <paramref name="entries"/>.
        /// </summary>
        /// <param name="feedItemId">The <see cref="FeedItemTo"/> of the current item</param>
        /// <param name="entries">The list of <see cref="TextEntry"/> to convert</param>
        /// <returns><see cref="FeedItem"/></returns>
        private FeedItem ToFeedItem(FeedItemId feedItemId, List<TextEntry> entries)
        {
            var messages = new List<FeedMessage>(entries.Count);
            foreach (var entry in entries)
            {
                var body = FeedMessageBody.With(entry.EntryData);
                var message = FeedMessage.With(entry.Id, body, entry.TypeName, entry.TypeVersion);
                messages.Add(message);
            }

            if (_feed.MessagesPerFeedItem == entries.Count)
            {
                return FeedItem.ArchivedFeedItemWith(feedItemId, feedItemId.Next(), feedItemId.Previous(), messages);
            }

            return FeedItem.CurrentFeedWith(feedItemId, feedItemId.Previous(), messages);
        }
    }
}