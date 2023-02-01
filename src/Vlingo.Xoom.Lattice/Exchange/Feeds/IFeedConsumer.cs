// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Exchange.Feeds;

/// <summary>
/// The protocol used by parties interested in consuming a <see cref="FeedItem"/> stream provided by a <see cref="IFeeder"/>.
/// </summary>
public interface IFeedConsumer
{
    /// <summary>
    /// Consumes the <see cref="FeedItem"/> requested of the <code>IFeeder.FeedItemTo(FeedItemId, FeedConsumer)</code>.
    /// </summary>
    /// <param name="feedItem">The <see cref="FeedItem"/> requested of the <see cref="IFeeder"/> to be consumed</param>
    void ConsumeFeedItem(FeedItem feedItem);
}