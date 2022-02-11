// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Lattice.Exchange.Feeds;

namespace Vlingo.Xoom.Lattice.Tests.Exchange.Feeds;

public class MockFeedConsumer : IFeedConsumer
{
    private AccessSafely _access;
    public readonly Dictionary<long, FeedItem> FeedItems = new Dictionary<long, FeedItem>();
        
    public void ConsumeFeedItem(FeedItem feedItem) => _access.WriteUsing("feedItems", feedItem);

    public AccessSafely AfterCompleting(int times)
    {
        _access = AccessSafely
            .AfterCompleting(times)
            .WritingWith<FeedItem>("feedItems", feedItem => FeedItems.Add(feedItem.Id.ToLong(), feedItem))
            .ReadingWith("feedItems", () => FeedItems);

        return _access;
    }
}