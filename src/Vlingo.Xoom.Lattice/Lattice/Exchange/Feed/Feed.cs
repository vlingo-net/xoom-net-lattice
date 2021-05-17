// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;

namespace Vlingo.Xoom.Lattice.Exchange.Feed
{
    /// <summary>
    /// Provides support utilities for <see cref="Feed"/> and related types.
    /// Every <see cref="Feed"/> has an <code>ExchangeName</code>.
    /// </summary>
    public abstract class Feed
    {
        /// <summary>
        /// Gets the <see cref="IEntryReader"/> to provide entries to the <see cref="IFeeder"/>.
        /// </summary>
        public abstract IEntryReader EntryReaderType { get; }

        /// <summary>
        /// Gets the type to be used by the <see cref="IFeeder"/>.
        /// </summary>
        public abstract Type FeederType { get; }

        /// <summary>
        /// Gets the <see cref="IFeeder"/> per <code>EntryReaderType</code> and <code>FeederType</code>.
        /// </summary>
        public abstract IFeeder Feeder { get; }

        /// <summary>
        /// Gets the defined number of message per feed. In not defined gets the default number of messages per feed
        /// </summary>
        public int MessagesPerFeedItem { get; } = 20;

        /// <summary>
        /// Gets the exchange name
        /// </summary>
        public abstract string ExchangeName { get; }

        /// <summary>
        /// Gets a new <see cref="Feed"/> with the given properties.
        /// </summary>
        /// <param name="stage">The <see cref="Stage"/> used to create this feeder</param>
        /// <param name="exchangeName">The name of this exchange</param>
        /// <param name="feederType">The <see cref="Actor"/> type of this feeder</param>
        /// <param name="entryReaderType">The <see cref="IEntryReader"/> that this feeder uses</param>
        /// <returns><see cref="Feed"/></returns>
        public static Feed DefaultFeedWith(Stage stage, string exchangeName, Type feederType, IEntryReader entryReaderType) =>
            new DefaultFeed(stage, exchangeName, feederType, entryReaderType);

        /// <summary>
        /// Gets the encoded identity for the <see cref="FeedItemId"/>.
        /// </summary>
        /// <param name="feedItemId">The value of the <see cref="FeedItem"/> identity</param>
        /// <returns><see cref="FeedItemId"/></returns>
        public virtual FeedItemId ItemId(long feedItemId) => ItemId(feedItemId.ToString());
        
        /// <summary>
        /// Gets the encoded identity for the <see cref="FeedItemId"/>.
        /// </summary>
        /// <param name="feedItemId">The value of the <see cref="FeedItem"/> identity</param>
        /// <returns><see cref="FeedItemId"/></returns>
        public virtual FeedItemId ItemId(string feedItemId) => new FeedItemId(feedItemId);

        /// <summary>
        /// Gets the type name for the message associated with the <see cref="IEntry"/>.
        /// </summary>
        /// <param name="entry">the <see cref="IEntry"/> used to determine the type name</param>
        /// <returns>The name of the message type</returns>
        public virtual string MessageTypeNameFrom(IEntry entry) => entry.TypeName;

        /// <summary>
        /// Gets the type name for the message associated with the <see cref="IEntry"/>.
        /// </summary>
        /// <param name="source">the <see cref="Source{T}"/> used to determine the type name</param>
        /// <returns>The name of the message type</returns>
        public virtual string MessageTypeNameFrom(ISource source) => source.GetType().Name;
    }
}