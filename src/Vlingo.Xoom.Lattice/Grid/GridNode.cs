// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
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
using Vlingo.Xoom.Lattice.Grid.Application;
using Vlingo.Xoom.Lattice.Grid.Application.Message;
using Vlingo.Xoom.Lattice.Grid.Application.Message.Serialization;
using Vlingo.Xoom.Lattice.Util;
using Vlingo.Xoom.Wire.Fdx.Outbound;
using Vlingo.Xoom.Wire.Message;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid
{
    public class GridNode : ClusterApplicationAdapter
    {
        // Sent messages waiting for continuation (answer) onto current node
        private static ConcurrentDictionary<Guid, UnAckMessage> correlationMessages = new ConcurrentDictionary<Guid, UnAckMessage>();

        private IAttributesProtocol client;
        private IGridRuntime gridRuntime;
        private Node localNode;

        private IOutbound outbound;

        private IInbound inbound;

        private IApplicationMessageHandler applicationMessageHandler;

        private List<IQuorumObserver> quorumObservers;
        
        public GridNode(IGridRuntime gridRuntime, Node localNode)
        {
            this.gridRuntime = gridRuntime;
            this.localNode = localNode;

            //var conf = FSTConfiguration.createDefaultConfiguration();
            // set classloader with available proxy classes
            //conf.setClassLoader(gridRuntime.WorldClassLoader);

            var holder = gridRuntime.World.ActorFor<IHardRefHolder>(Actors.Definition.Has(() => new ExpiringHardRefHolder()));

            this.outbound =
                Stage.ActorFor<IOutbound>(
                    () => new OutboundGridActorControl(
            localNode.Id,
                new JsonEncoder(),
            (id, message) => correlationMessages.AddOrUpdate(id, i => (UnAckMessage) message, (i, v) => (UnAckMessage) message),
                new OutBuffers(holder)));

            this.gridRuntime.SetOutbound(outbound);

            this.inbound =
                Stage.ActorFor<IInbound>(
                    () => 
            new InboundGridActorControl(
                gridRuntime,
                id => correlationMessages.Remove(id)));

            this.applicationMessageHandler =
                new GridApplicationMessageHandler(
                    localNode.Id,
                    gridRuntime.HashRing,
                    inbound,
                    outbound,
                    new JsonDecoder(),
                    holder,
                    Scheduler,
                    Logger);

            this.quorumObservers = new List<IQuorumObserver>(3);

            RegisterQuorumObserver(gridRuntime);
        }

        public override void Start()
        {
            Logger.Debug($"GRID: Started on node: {localNode}");
            gridRuntime.HashRing.IncludeNode(localNode.Id);
        }

        public void RegisterQuorumObserver(IQuorumObserver observer) => this.quorumObservers.Add(observer);

        public override void HandleApplicationMessage(RawMessage message)
        {
            Logger.Debug($"GRID: Received application message: {message.AsTextMessage()}");
            applicationMessageHandler.Handle(message);
        }

        public override void InformAllLiveNodes(IEnumerable<Node> liveNodes, bool isHealthyCluster) => 
            Logger.Debug($"GRID: Live nodes confirmed: {liveNodes} and is healthy: {isHealthyCluster}");

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
            outbound.InformNodeIsHealthy(nodeId, isHealthyCluster);
            applicationMessageHandler.InformNodeIsHealthy(nodeId, isHealthyCluster);
        }

        public override void InformNodeJoinedCluster(Id nodeId, bool isHealthyCluster)
        {
            Logger.Debug($"GRID: Node joined: {nodeId} and is healthy: {isHealthyCluster}");
            gridRuntime.NodeJoined(nodeId);
        }

        public override void InformNodeLeftCluster(Id nodeId, bool isHealthyCluster)
        {
            Logger.Debug($"GRID: Node left: {nodeId} and is healthy: {isHealthyCluster}");
            outbound.InformNodeIsHealthy(nodeId, isHealthyCluster);
            applicationMessageHandler.InformNodeIsHealthy(nodeId, isHealthyCluster);
            gridRuntime.HashRing.ExcludeNode(nodeId);
            RetryUnAckMessagesOn(nodeId);
        }

        public override void InformQuorumAchieved()
        {
            Logger.Debug("GRID: Quorum achieved");
            quorumObservers.ForEach(quorumObserver => quorumObserver.QuorumAchieved());
        }

        public override void InformQuorumLost()
        {
            Logger.Debug("GRID: Quorum lost");
            quorumObservers.ForEach(quorumObserver => quorumObserver.QuorumLost());
        }

        public override void InformResponder(IApplicationOutboundStream? responder) => this.outbound.UseStream(responder);

        public override void InformAttributesClient(IAttributesProtocol client)
        {
            Logger.Debug("GRID: Attributes Client received.");
            this.client = client;
        }

        public override void InformAttributeSetCreated(string? attributeSetName) => 
            Logger.Debug($"GRID: Attributes Set Created: {attributeSetName}");

        public override void InformAttributeAdded(string attributeSetName, string? attributeName)
        {
            var attr = client.Attribute<string>(attributeSetName, attributeName);
            Logger.Debug($"GRID: Attribute Set {attributeSetName} Attribute Added: {attributeName} Value: {attr.Value}");
        }

        public override void InformAttributeRemoved(string attributeSetName, string? attributeName)
        {
            var attr = client.Attribute<string>(attributeSetName, attributeName);
            Logger.Debug($"GRID: Attribute Set {attributeSetName} Attribute Removed: {attributeName} Attribute: {attr}");
        }

        public override void InformAttributeSetRemoved(string? attributeSetName) =>
            Logger.Debug($"GRID: Attributes Set Removed: {attributeSetName}");

        public override void InformAttributeReplaced(string attributeSetName, string? attributeName)
        {
            var attr = client.Attribute<string>(attributeSetName, attributeName);
            Logger.Debug($"GRID: Attribute Set {attributeSetName} Attribute Replaced: {attributeName} Value: {attr.Value}");
        }

        public override void Stop()
        {
            if (!IsStopped)
            {
                Logger.Debug("GRID: Stopping...");
                gridRuntime.RelocateActors();
                base.Stop();
            }
        }

        /// <summary>
        /// Retry unacknowledged messages onto a new node (recipient).
        /// </summary>
        /// <param name="leftNode">The node that left the cluster</param>
        private void RetryUnAckMessagesOn(Id leftNode)
        {
            var retryMessages = correlationMessages
                .Where(entry => leftNode.Equals(entry.Value.Receiver))
                .ToDictionary(kv => kv.Key, pair => pair.Value);

            retryMessages.Keys.ToList().ForEach(id => correlationMessages.Remove(id));

            foreach (var retryMessage in retryMessages.Values)
            {
                var deliver = retryMessage.Message;
                var newRecipient = gridRuntime.HashRing.NodeOf(deliver.Address.IdString);

                if (newRecipient.Equals(localNode.Id))
                {
                    inbound.Deliver(newRecipient,
                        newRecipient,
                        retryMessage.Completes,
                        deliver.Protocol,
                        deliver.Address,
                        deliver.Definition,
                        deliver.Consumer,
                        deliver.Representation);
                }
                else
                {
                    outbound.Deliver(newRecipient,
                        localNode.Id,
                        retryMessage.Completes,
                        deliver.Protocol,
                        deliver.Address,
                        deliver.Definition,
                        deliver.Consumer,
                        deliver.Representation);
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
}