// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Cluster.Model.Application;
using Vlingo.Xoom.Cluster.Model.Attribute;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Grid.Application;
using Vlingo.Xoom.Lattice.Grid.Application.Message;
using Vlingo.Xoom.Lattice.Grid.Application.Message.Serialization;
using Vlingo.Xoom.Lattice.Util;
using Vlingo.Xoom.Wire.Fdx.Outbound;
using Vlingo.Xoom.Wire.Message;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid;

public class GridNode : ClusterApplicationAdapter
{
    // Sent messages waiting for continuation (answer) onto current node
    private static readonly ConcurrentDictionary<Guid, UnAckMessage> GridMessagesCorrelations = new ConcurrentDictionary<Guid, UnAckMessage>();
    private static readonly ConcurrentDictionary<Guid, ICompletes> ActorMessagesCorrelations = new ConcurrentDictionary<Guid, ICompletes>();

    private IAttributesProtocol? _client;
    private readonly IGridRuntime _gridRuntime;
    private readonly Node _localNode;

    private readonly IOutbound _outbound;

    private readonly IInbound _inbound;

    private readonly IApplicationMessageHandler _applicationMessageHandler;

    private readonly List<IQuorumObserver> _quorumObservers;
        
    public GridNode(IGridRuntime gridRuntime, Node localNode)
    {
        _gridRuntime = gridRuntime;
        _localNode = localNode;
            
        _gridRuntime.NodeId =localNode.Id;

        //var conf = FSTConfiguration.createDefaultConfiguration();
        // set classloader with available proxy classes
        //conf.setClassLoader(gridRuntime.WorldClassLoader);

        var holder = gridRuntime.World.ActorFor<IHardRefHolder>(Actors.Definition.Has(() => new ExpiringHardRefHolder()));

        _outbound =
            Stage.ActorFor<IOutbound>(
                () => new OutboundGridActorControl(
                    localNode.Id,
                    new JsonEncoder(),
                    (id, message) => GridMessagesCorrelations.AddOrUpdate(id, i => (UnAckMessage) message, (i, v) => (UnAckMessage) message),
                    (id, message) => ActorMessagesCorrelations.AddOrUpdate(id, i => message, (i, v) => message),
                    new OutBuffers(holder)));

        _gridRuntime.Outbound = _outbound;

        _inbound =
            Stage.ActorFor<IInbound>(
                () => new InboundGridActorControl(
                    gridRuntime,
                    id => GridMessagesCorrelations.Remove(id),
                    id => ActorMessagesCorrelations.Remove(id)));

        _applicationMessageHandler =
            new GridApplicationMessageHandler(
                localNode.Id,
                gridRuntime.HashRing,
                _inbound,
                _outbound,
                new JsonDecoder(),
                holder,
                Scheduler,
                Logger);

        _quorumObservers = new List<IQuorumObserver>(3);

        RegisterQuorumObserver(gridRuntime);
    }

    public override void Start()
    {
        Logger.Debug($"GRID: Started on node: {_localNode}");
        _gridRuntime.HashRing.IncludeNode(_localNode.Id);
    }

    public void RegisterQuorumObserver(IQuorumObserver observer) => _quorumObservers.Add(observer);

    public override void HandleApplicationMessage(RawMessage message)
    {
        Logger.Debug($"GRID: Received application message: {message.AsTextMessage()}");
        _applicationMessageHandler.Handle(message);
    }

    public override void InformAllLiveNodes(IEnumerable<Node> liveNodes, bool isHealthyCluster)
    {
        Logger.Debug($"GRID: Live nodes confirmed: {liveNodes} and is healthy: {isHealthyCluster}");
        _gridRuntime.InformAllLiveNodes(liveNodes);
    }

    public override void InformLeaderElected(Id leaderId, bool isHealthyCluster, bool isLocalNodeLeading)
    {
        Logger.Debug($"GRID: Leader elected: {leaderId} and is healthy: {isHealthyCluster}");

        if (isLocalNodeLeading)
        {
            Logger.Debug("GRID: Local node is leading.");
        }
    }

    public override void InformLeaderLost(Id lostLeaderId, bool isHealthyCluster) => 
        Logger.Debug($"GRID: Leader lost: {lostLeaderId} and is healthy: {isHealthyCluster}");

    public override void InformLocalNodeShutDown(Id nodeId)
    {
        Logger.Debug($"GRID: Local node shut down: {nodeId}");
        // TODO relocate local actors to another node?
    }

    public override void InformLocalNodeStarted(Id nodeId) => Logger.Debug($"GRID: Local node started: {nodeId}");

    public override void InformNodeIsHealthy(Id nodeId, bool isHealthyCluster)
    {
        Logger.Debug($"GRID: Node reported healthy: {nodeId} and is healthy: {isHealthyCluster}");
        _outbound.InformNodeIsHealthy(nodeId, isHealthyCluster);
        _applicationMessageHandler.InformNodeIsHealthy(nodeId, isHealthyCluster);
    }

    public override void InformNodeJoinedCluster(Id nodeId, bool isHealthyCluster)
    {
        Logger.Debug($"GRID: Node joined: {nodeId} and is healthy: {isHealthyCluster}");
        _gridRuntime.NodeJoined(nodeId);
    }

    public override void InformNodeLeftCluster(Id nodeId, bool isHealthyCluster)
    {
        Logger.Debug($"GRID: Node left: {nodeId} and is healthy: {isHealthyCluster}");
        _outbound.InformNodeIsHealthy(nodeId, isHealthyCluster);
        _applicationMessageHandler.InformNodeIsHealthy(nodeId, isHealthyCluster);
        _gridRuntime.HashRing.ExcludeNode(nodeId);
        RetryUnAckMessagesOn(nodeId);
    }

    public override void InformQuorumAchieved()
    {
        Logger.Debug("GRID: Quorum achieved");
        _quorumObservers.ForEach(quorumObserver => quorumObserver.QuorumAchieved());
    }

    public override void InformQuorumLost()
    {
        Logger.Debug("GRID: Quorum lost");
        _quorumObservers.ForEach(quorumObserver => quorumObserver.QuorumLost());
    }

    public override void InformResponder(IApplicationOutboundStream? responder) => _outbound.UseStream(responder);

    public override void InformAttributesClient(IAttributesProtocol client)
    {
        Logger.Debug("GRID: Attributes Client received.");
        _client = client;
    }

    public override void InformAttributeSetCreated(string? attributeSetName) => 
        Logger.Debug($"GRID: Attributes Set Created: {attributeSetName}");

    public override void InformAttributeAdded(string attributeSetName, string? attributeName)
    {
        var attr = _client?.Attribute<string>(attributeSetName, attributeName);
        Logger.Debug($"GRID: Attribute Set {attributeSetName} Attribute Added: {attributeName} Value: {attr?.Value}");
    }

    public override void InformAttributeRemoved(string attributeSetName, string? attributeName)
    {
        var attr = _client?.Attribute<string>(attributeSetName, attributeName);
        Logger.Debug($"GRID: Attribute Set {attributeSetName} Attribute Removed: {attributeName} Attribute: {attr}");
    }

    public override void InformAttributeSetRemoved(string? attributeSetName) =>
        Logger.Debug($"GRID: Attributes Set Removed: {attributeSetName}");

    public override void InformAttributeReplaced(string attributeSetName, string? attributeName)
    {
        var attr = _client?.Attribute<string>(attributeSetName, attributeName);
        Logger.Debug($"GRID: Attribute Set {attributeSetName} Attribute Replaced: {attributeName} Value: {attr?.Value}");
    }

    public override void Stop()
    {
        if (!IsStopped)
        {
            Logger.Debug("GRID: Stopping...");
            _gridRuntime.RelocateActors();
            base.Stop();
        }
    }

    /// <summary>
    /// Retry unacknowledged messages onto a new node (recipient).
    /// </summary>
    /// <param name="leftNode">The node that left the cluster</param>
    private void RetryUnAckMessagesOn(Id leftNode)
    {
        var retryMessages = GridMessagesCorrelations
            .Where(entry => leftNode.Equals(entry.Value.Receiver))
            .ToDictionary(kv => kv.Key, pair => pair.Value);

        retryMessages.Keys.ToList().ForEach(id => GridMessagesCorrelations.Remove(id));

        foreach (var retryMessage in retryMessages.Values)
        {
            var gridDeliver = retryMessage.Message;
            var newRecipient = _gridRuntime.HashRing.NodeOf(gridDeliver.Address.IdString);

            if (newRecipient.Equals(_localNode.Id))
            {
                _inbound.GridDeliver(newRecipient,
                    newRecipient,
                    retryMessage.Completes,
                    gridDeliver.Protocol,
                    gridDeliver.Address,
                    gridDeliver.Definition,
                    gridDeliver.Consumer,
                    gridDeliver.Representation);
            }
            else
            {
                _outbound.GridDeliver(newRecipient,
                    _localNode.Id,
                    retryMessage.Completes,
                    gridDeliver.Protocol,
                    gridDeliver.Address,
                    gridDeliver.Definition,
                    gridDeliver.Consumer,
                    gridDeliver.Representation);
            }
        }
    }
}

public static class ConcurrentDictionaryExtensions
{
    public static TValue Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey id) where TKey : notnull
    {
        if (dictionary.TryRemove(id, out var value))
        {
            return value;
        }

        return default!;
    }
}