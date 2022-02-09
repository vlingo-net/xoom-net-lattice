// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Exchange.Feeds
{
    /// <summary>
    /// I provide <see cref="FeedItem"/> to a given <see cref="IFeedConsumer"/>.
    /// </summary>
    public interface IFeeder
    {
        /// <summary>
        /// Sends the <see cref="FeedItem"/> identified by <see cref="FeedItemId"/> to <see cref="IFeedConsumer"/>.
        /// </summary>
        /// <param name="fromFeedItemId">The <see cref="FeedItemId"/> of the <see cref="FeedItem"/> to send</param>
        /// <param name="feedConsumer">The <see cref="IFeedConsumer"/> to which the requested <see cref="FeedItem"/> is sent</param>
        void FeedItemTo(FeedItemId fromFeedItemId, IFeedConsumer feedConsumer);
    }
}