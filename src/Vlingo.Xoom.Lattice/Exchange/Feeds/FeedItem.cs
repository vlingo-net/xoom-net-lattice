// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Lattice.Exchange.Feeds
{
    /// <summary>
    /// A single feed item of messages including links to next and previous items.
    /// If considered archived by <code>Archived</code> indicator will be <value>true</value>.
    /// An archived feed item is one that will not change in the future; it is full.
    /// </summary>
    public class FeedItem
    {
        /// <summary>
        /// Gets an indicator of whether this feed is considered archived or not
        /// </summary>
        public bool Archived { get; }

        /// <summary>
        /// The current identity
        /// </summary>
        public FeedItemId Id { get; }

        /// <summary>
        /// All messages
        /// </summary>
        public IEnumerable<FeedMessage> Messages { get; }

        /// <summary>
        /// Gets the identity of the next entry, if known
        /// </summary>
        public FeedItemId NextId { get; }

        /// <summary>
        /// Gets the identity of the previous entry, if any
        /// </summary>
        public FeedItemId PreviousId { get; }

        /// <summary>
        /// Construct my state with identities, messages, and archive indicator.
        /// </summary>
        /// <param name="id">The <see cref="FeedItemId"/> identity</param>
        /// <param name="nextId">The <see cref="FeedItemId"/> identity of the next FeedItem, if known</param>
        /// <param name="previousId">The identity of the previous feed, if any</param>
        /// <param name="messages">The <see cref="List{T}"/> of messages</param>
        /// <param name="archived">The boolean indicator that this feed is considered archived or not</param>
        /// <exception cref="ArgumentNullException">If any of the arguments is null</exception>
        private FeedItem(FeedItemId id, FeedItemId nextId, FeedItemId previousId, List<FeedMessage> messages, bool archived)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (nextId == null)
            {
                throw new ArgumentNullException(nameof(nextId));
            }
            if (previousId == null)
            {
                throw new ArgumentNullException(nameof(previousId));
            }
            
            Id = id;
            NextId = nextId;
            PreviousId = previousId;
            
            Messages = messages;
            Archived = archived;
        }

        /// <summary>
        /// Construct my state with identities, messages, and <code>false</code> archive indicator
        /// </summary>
        /// <param name="id">The <see cref="FeedItemId"/> identity</param>
        /// <param name="previousFeedId">The identity of the previous feed, if any</param>
        /// <param name="messages">The <see cref="List{T}"/> of messages</param>
        private FeedItem(FeedItemId id, FeedItemId previousFeedId, List<FeedMessage> messages) : this(id, FeedItemId.Unknown, previousFeedId, messages, false)
        {
        }

        /// <summary>
        /// Gets the new <see cref="FeedItem"/> that is considered current/head, with
        /// <paramref name="id"/>, a <paramref name="previousId"/>, and <paramref name="messages"/>.
        /// </summary>
        /// <param name="id">The <see cref="FeedItemId"/> identity</param>
        /// <param name="nextId">The <see cref="FeedItemId"/> identity of the next FeedItem, if known</param>
        /// <param name="previousId">The identity of the previous feed, if any</param>
        /// <param name="messages">The <see cref="List{T}"/> of messages</param>
        /// <returns><see cref="FeedItem"/></returns>
        public static FeedItem ArchivedFeedItemWith(FeedItemId id, FeedItemId nextId, FeedItemId previousId, IEnumerable<FeedMessage> messages) => 
            new FeedItem(id, nextId, previousId, messages.ToList(), true);

        /// <summary>
        /// Gets the new <see cref="FeedItem"/> that is considered current/head, with
        /// <paramref name="id"/>, a <paramref name="previousId"/>, and <paramref name="messages"/>.
        /// </summary>
        /// <param name="id">The <see cref="FeedItemId"/> identity</param>
        /// <param name="previousId">The identity of the previous feed, if any</param>
        /// <param name="messages">The <see cref="List{T}"/> of messages</param>
        /// <returns><see cref="FeedItem"/></returns>
        public static FeedItem CurrentFeedWith(FeedItemId id, FeedItemId previousId, IEnumerable<FeedMessage> messages) => 
            new FeedItem(id, previousId, messages.ToList());
    }
}