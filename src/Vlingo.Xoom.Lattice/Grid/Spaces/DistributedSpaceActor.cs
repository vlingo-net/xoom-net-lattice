// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Linq.Expressions;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces;

public class DistributedSpaceActor : Actor, IDistributedSpace
{
    private readonly string _accessorName;
    private readonly string _spaceName;
    private readonly int _totalPartitions;
    private readonly TimeSpan _scanInterval;
    // Write through factor on all the other distributed nodes. 0.0f value means no write through, only write eventually.
    // * 1.0f value means write through on all the other distributed nodes.
    private readonly float _writeThroughFactor;
    private readonly ISpace _localSpace;
    private readonly Grid _grid;

    public DistributedSpaceActor(
        string accessorName,
        string spaceName,
        int totalPartitions,
        TimeSpan scanInterval,
        float writeThroughFactor,
        ISpace localSpace,
        Grid grid)
    {
        _accessorName = accessorName;
        _spaceName = spaceName;
        _totalPartitions = totalPartitions;
        _scanInterval = scanInterval;
        _writeThroughFactor = writeThroughFactor;
        _localSpace = localSpace;
        _grid = grid;
    }
        
    public ICompletes<KeyItem> LocalPut(IKey key, Item item)
    {
        // this method is invoked remotely as well
        Logger.Debug($"Local PUT for {key} and {item.Object}");
        return _localSpace.Put(key, item)
            .AndThen(keyItem => {
                CompletesEventually().With(keyItem);
                return keyItem;
            });
    }

    public ICompletes<KeyItem> LocalTake(IKey key, Period until)
    {
        // this method is invoked remotely as well
        Logger.Debug($"Local TAKE for {key}");
        var localSpaceCompletes = _localSpace.Take(key, until);

        return localSpaceCompletes.AndThen(keyItem => {
            // Optional is not Serializable; return possibly null instead
            var maybeNull = keyItem.OrElse(null!);
            CompletesEventually().With(maybeNull);
            return maybeNull;
        });
    }
        
    public ICompletes<T> ItemFor<T>(Type actorType, params object[] parameters)
    {
        // see also SpaceItemFactoryRelay#itemFor
        var actor = _grid.ActorFor<T>(actorType,Definition.Has(actorType, parameters.ToArray()), _grid.AddressFactory.Unique());
        return Completes<T>().With(actor);
    }

    public ICompletes<KeyItem> Put(IKey key, Item item)
    {
        Expression<Action<IDistributedSpace>> consumer = actor => actor.LocalPut(key, item);
        var outbound = _grid.Outbound;
        var representation = "LocalPut(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Vlingo.Xoom.Lattice.Grid.Spaces.Item)"; // see DistributedSpace__Proxy
        var actorProvider = NewActorProvider();

        var allOtherNodes = _grid.AllOtherNodes();
        var writeThroughNodes = WriteThroughOtherNodes(_writeThroughFactor, allOtherNodes.Count) + 1;
        var completedWriteThrough = new AtomicInteger(0);

        Logger.Debug($"Local PUT for {key} and {item.Object}");
        var localSpaceCompletes = _localSpace.Put(key, item);

        // make sure put on local space happens first before any replication on other nodes
        return localSpaceCompletes.AndThen(localKeyItem => {
            foreach (var nodeId in _grid.AllOtherNodes())
            {
                var distributedCompletes = Common.Completes.Using<KeyItem>(Scheduler);
                distributedCompletes.AndThenConsume(remoteKeyItem => {
                    Logger.Debug(
                        $"Confirmation of distributed space PUT for {remoteKeyItem.Key} with {remoteKeyItem.Object} from {nodeId}");
                    if (completedWriteThrough.IncrementAndGet() == writeThroughNodes)
                    {
                        CompletesEventually().With(localKeyItem);
                    }
                });

                outbound?.ActorDeliver(nodeId,
                    _grid.NodeId!,
                    Common.Completes.WithSuccess(distributedCompletes),
                    typeof(IDistributedSpace),
                    actorProvider,
                    consumer,
                    representation);
            }

            if (completedWriteThrough.IncrementAndGet() == writeThroughNodes)
            {
                CompletesEventually().With(localKeyItem);
            }

            return localKeyItem;
        });
    }

    public ICompletes<Optional<KeyItem>> Get(IKey key, Period until)
    {
        var localSpaceCompletes = _localSpace.Get(key, until);

        return localSpaceCompletes.AndThen(keyItem => {
            CompletesEventually().With(keyItem);
            return keyItem;
        });
    }

    public ICompletes<Optional<KeyItem>> Take(IKey key, Period until)
    {
        Expression<Action<IDistributedSpace>> consumer = actor => actor.LocalTake(key, until);
        var outbound = _grid.Outbound;
        var representation = "LocalTake(Vlingo.Xoom.Lattice.Grid.Spaces.IKey, Vlingo.Xoom.Lattice.Grid.Spaces.Period)"; // see DistributedSpace__Proxy
        var actorProvider = NewActorProvider();

        var allOtherNodes = _grid.AllOtherNodes();
        var writeThroughNodes = WriteThroughOtherNodes(_writeThroughFactor, allOtherNodes.Count) + 1;
        var completedWriteThrough = new AtomicInteger(0);

        Logger.Debug($"Local TAKE for {key}");
        var localSpaceCompletes = _localSpace.Take(key, until);

        // make sure take on local space happens first before any replication on other nodes
        return localSpaceCompletes.AndThen(localKeyItem => {
            foreach (var nodeId in allOtherNodes)
            {
                var distributedCompletes = Common.Completes.Using<KeyItem>(Scheduler);
                distributedCompletes.AndThenConsume(maybeNull => {
                    Logger.Debug($"Confirmation of distributed space TAKE from {nodeId}");
                    if (completedWriteThrough.IncrementAndGet() == writeThroughNodes)
                    {
                        CompletesEventually().With(localKeyItem);
                    }
                });

                outbound?.ActorDeliver(nodeId,
                    _grid.NodeId!,
                    Common.Completes.WithSuccess(distributedCompletes),
                    typeof(IDistributedSpace),
                    actorProvider,
                    consumer,
                    representation);
            }

            if (completedWriteThrough.IncrementAndGet() == writeThroughNodes)
            {
                CompletesEventually().With(localKeyItem);
            }

            return localKeyItem;
        });
    }
        
    private Func<Grid, Actor> NewActorProvider()
    {
        var localAccessorName = _accessorName;
        var localSpaceName = _spaceName;
        var localTotalPartitions = _totalPartitions;
        var localScanInterval = _scanInterval;

        return g => {
            var maybeAccessor = Accessor.Named(g, localAccessorName);
            var accessor = maybeAccessor.IsDefined
                ? maybeAccessor
                : Accessor.Using(g, localAccessorName);

            return (Actor) accessor.DistributedSpaceFor(localSpaceName, localTotalPartitions, localScanInterval);
        };
    }
        
    private int WriteThroughOtherNodes(float writeThroughFactor, int allOtherNodes) =>
        (int)Math.Min(allOtherNodes,
            Math.Round(writeThroughFactor * allOtherNodes));
}