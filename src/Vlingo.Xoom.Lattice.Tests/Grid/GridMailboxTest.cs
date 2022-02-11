// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Vlingo.Xoom.Actors.Plugin.Mailbox.TestKit;
using Vlingo.Xoom.Lattice.Grid;
using Vlingo.Xoom.Lattice.Grid.Cache;
using Vlingo.Xoom.Lattice.Grid.Hashring;
using Vlingo.Xoom.Wire.Nodes;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Grid;

public class GridMailboxTest
{
    private static readonly Cache Cache = Cache.DefaultCache();
    private readonly Func<int, Id, HashedNodePoint<Id>> _factory =  (hash, node) => new CacheNodePoint<Id>(Cache, hash, node);
        
    [Fact]
    public void TestMurmurArrayHashRing()
    {
        var hashRing = new MurmurArrayHashRing<Id>(100, _factory);

        var localId = Id.Of(1);
        var localMailbox = new TestMailbox();
        var address = new GridAddress(Guid.NewGuid());

        hashRing.IncludeNode(localId);
        hashRing.IncludeNode(Id.Of(2));
        hashRing.IncludeNode(Id.Of(3));
        hashRing.IncludeNode(Id.Of(4));

        var gridMailbox = new GridMailbox(localMailbox, localId, address, hashRing, null, ConsoleLogger.TestInstance());

        Assert.False(gridMailbox.IsClosed);
        gridMailbox.Close();
        Assert.True(gridMailbox.IsClosed);
        Assert.True(gridMailbox.ConcurrencyCapacity > 0);
    }

    [Fact]
    public void TestMD5ArrayHashRing()
    {
        var hashRing = new MD5ArrayHashRing<Id>(100, _factory);

        var localId = Id.Of(1);
        var localMailbox = new TestMailbox();
        var address = new GridAddress(Guid.NewGuid());

        hashRing.IncludeNode(localId);
        hashRing.IncludeNode(Id.Of(2));

        var gridMailbox = new GridMailbox(localMailbox, localId, address, hashRing, null, ConsoleLogger.TestInstance());

        Assert.False(gridMailbox.IsClosed);
        gridMailbox.Close();
        Thread.Sleep(10);
        Assert.True(gridMailbox.IsClosed);
        Assert.True(gridMailbox.ConcurrencyCapacity > 0);
    }
}