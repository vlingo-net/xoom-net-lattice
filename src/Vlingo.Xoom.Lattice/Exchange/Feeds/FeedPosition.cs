// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Exchange.Feeds
{
    /// <summary>
    ///     Holder of Feed exchange and subscriber position information.
    /// </summary>
    public class FeedPosition
    {
        /// <summary>
        /// Constructs default <see cref="FeedPosition"/>
        /// </summary>
        /// <param name="exchangeName">The name of the exchange</param>
        /// <param name="subscriberId">The name of the subscriber</param>
        /// <param name="feedItem">The <see cref="FeedItem"/> indicating the current position</param>
        /// <exception cref="ArgumentNullException">If <paramref name="exchangeName"/> or <paramref name="subscriberId"/> is null or empty</exception>
        public FeedPosition(string exchangeName, string subscriberId, FeedItem feedItem)
        {
            if (exchangeName == null || string.IsNullOrEmpty(exchangeName))
            {
                throw new ArgumentNullException(nameof(exchangeName));
            }

            if (subscriberId == null || string.IsNullOrEmpty(subscriberId))
            {
                throw new ArgumentNullException(nameof(subscriberId));
            }

            ExchangeName = exchangeName;
            SubscriberId = subscriberId;
            FeedItem = feedItem;
        }

        /// <summary>
        /// Gets the name of the exchange
        /// </summary>
        public string ExchangeName { get; }

        /// <summary>
        /// Gets the identity indicating the current position
        /// </summary>
        public FeedItem FeedItem { get; }

        /// <summary>
        /// Gets the identity of the subscriber
        /// </summary>
        public string SubscriberId { get; }

        /// <summary>
        /// Gets a new <see cref="FeedPosition"/>
        /// </summary>
        /// <param name="exchangeName">The name of the exchange</param>
        /// <param name="subscriberId">The identity of the subscriber</param>
        /// <param name="feedItem">The <see cref="FeedItem"/> indicating the current position</param>
        /// <returns><see cref="FeedPosition"/></returns>
        public FeedPosition Is(string exchangeName, string subscriberId, FeedItem feedItem) =>
            new FeedPosition(exchangeName, subscriberId, feedItem);

        /// <summary>
        /// Gets a new <see cref="FeedPosition"/> with <paramref name="feedItem"/>.
        /// </summary>
        /// <param name="feedItem">The <see cref="FeedItem"/> indicating the current position</param>
        /// <returns><see cref="FeedPosition"/></returns>
        public FeedPosition With(FeedItem feedItem) =>
            new FeedPosition(ExchangeName, SubscriberId, feedItem);
    }
}