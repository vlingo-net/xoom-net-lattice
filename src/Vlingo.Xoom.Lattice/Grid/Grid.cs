// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common.Compiler;
using Vlingo.Xoom.Common.Identity;
using Vlingo.Xoom.Lattice.Grid.Application;
using Vlingo.Xoom.Lattice.Grid.Cache;
using Vlingo.Xoom.Lattice.Grid.Hashring;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid
{
    public class Grid : Stage, IGridRuntime
    {
        private static int GridStageBuckets = 32;
        private static int GridStageInitialCapacity = 16_384;

        private readonly ILogger _logger;

        private static string INSTANCE_NAME = Guid.NewGuid().ToString();
        
        private GridNodeBootstrap gridNodeBootstrap;
        private IHashRing<Id> hashRing;

        private Id nodeId;
        private IOutbound outbound;

        private volatile bool hasQuorum;
        private long clusterHealthCheckInterval;
        
        public static Grid Instance(World world) => world.ResolveDynamic<Grid>(INSTANCE_NAME);
        
        public static Grid Start(string worldName, string gridNodeName) => 
            Start(worldName, Configuration.Define(), Cluster.Model.Properties.Instance, gridNodeName);

        public static Grid Start(World world, string gridNodeName) => 
            Start(world, new GridAddressFactory(IdentityGeneratorType.Random), Cluster.Model.Properties.Instance, gridNodeName);

        public static Grid Start(string worldName, Configuration worldConfiguration, string gridNodeName) => 
            Start(worldName, worldConfiguration, Cluster.Model.Properties.Instance, gridNodeName);

        public static Grid Start(string worldName, Configuration worldConfiguration, Cluster.Model.Properties clusterProperties, string gridNodeName) => 
            Start(worldName, new GridAddressFactory(IdentityGeneratorType.Random), worldConfiguration, clusterProperties, gridNodeName);

        public static Grid Start(World world, Cluster.Model.Properties clusterProperties, string gridNodeName) => 
            Start(world, new GridAddressFactory(IdentityGeneratorType.Random), clusterProperties, gridNodeName);

        public static Grid Start(string worldName, IAddressFactory addressFactory, Configuration worldConfiguration, Cluster.Model.Properties clusterProperties, string gridNodeName)
        {
            var world = World.Start(worldName, worldConfiguration);
            return new Grid(world, addressFactory, clusterProperties, gridNodeName);
        }
        
        public static Grid Start(World world, IAddressFactory addressFactory, Cluster.Model.Properties clusterProperties, string gridNodeName) => 
            new Grid(world, addressFactory, clusterProperties, gridNodeName);

        protected internal Grid(World world, IAddressFactory addressFactory, Cluster.Model.Properties clusterProperties, string name)
            : this(world, addressFactory, clusterProperties, name, GridStageBuckets, GridStageInitialCapacity, world.DefaultLogger)
        {
        }

        protected internal Grid(World world, IAddressFactory addressFactory, Cluster.Model.Properties clusterProperties, string name, int directoryBuckets, int directoryInitialCapacity, ILogger logger)
            : base(world, addressFactory, name, directoryBuckets, directoryInitialCapacity)
        {
            _logger = logger;
            this.hashRing = new MurmurSortedMapHashRing<Id>(100, (i, id) => new CacheNodePoint<Id>(Cache.Cache.Of(name), i, id));
            ExtenderStartDirectoryScanner();
            this.gridNodeBootstrap = GridNodeBootstrap.Boot(this, name, clusterProperties, false);
            this.hasQuorum = false;

            this.clusterHealthCheckInterval = clusterProperties.ClusterHealthCheckInterval();

            world.RegisterDynamic(INSTANCE_NAME, this);
        }

        protected override Func<IAddress?, IMailbox?, IMailbox?> MailboxWrapper() => 
            (a, m) => new GridMailbox(m!, nodeId, a!, hashRing, outbound, _logger);
        
        public void Terminate() => World.Terminate();
        
        public void QuorumAchieved() => this.hasQuorum = true;

        public void QuorumLost() => this.hasQuorum = false;

        //====================================
        // GridRuntime
        //====================================
        
        /// <summary>
        /// Answers the Actor at the specified Address.
        /// </summary>
        /// <param name="address">The Address of the actor</param>
        /// <returns>The Actor</returns>
        public Actor ActorAt(IAddress address) => ActorOf(this, address)!;

        public void RelocateActors()
        {
            var copy = this.hashRing.Copy();
            this.hashRing.ExcludeNode(nodeId);

            AllActorAddresses(this)
                .Where(address => address.IsDistributable && IsAssignedTo(copy, address, nodeId))
                .ToList()
                .ForEach(address => {
                    var actor = ActorOf(this, address);
                    var toNode = hashRing.NodeOf(address.IdString);
                    if (toNode != null)
                    {
                        // last node in the cluster?
                        RelocateActorTo(actor, address, toNode);
                    }
                });
        }

        public Stage AsStage()
        {
            throw new System.NotImplementedException();
        }

        public void NodeJoined(Id newNode)
        {
            throw new System.NotImplementedException();
        }

        public void SetNodeId(Id nodeId)
        {
            throw new System.NotImplementedException();
        }

        public void SetOutbound(IOutbound outbound)
        {
            throw new System.NotImplementedException();
        }

        public GridNodeBootstrap GridNodeBootstrap { get; }
        public IHashRing<Id> HashRing { get; }
        public IQuorumObserver QuorumObserver { get; }
        public DynaClassLoader WorldClassLoader { get; }
        
        private void RelocateActorTo(Actor actor, IAddress address, Id toNode)
        {
            if (!GridActorOperations.IsSuspendedForRelocation(actor))
            {
                _logger.Debug($"Relocating actor [{address}] to [{toNode}]");
                //actor.suspendForRelocation();
                GridActorOperations.SuspendForRelocation(actor);
                outbound.Relocate(
                    toNode,
                    nodeId,
                    Definition.SerializationProxy.From(actor.Definition),
                    address,
                    GridActorOperations.SupplyRelocationSnapshot(actor) /*actor.provideRelocationSnapshot()*/,
                    GridActorOperations.Pending(actor));
            }
        }
        
        private static bool IsAssignedTo(IHashRing<Id> ring, IAddress a, Id node) => 
            node.Equals(ring.NodeOf(a.IdString));
    }
}