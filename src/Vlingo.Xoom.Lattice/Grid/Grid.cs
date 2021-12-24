// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common.Compiler;
using Vlingo.Xoom.Common.Identity;
using Vlingo.Xoom.Lattice.Grid.Application;
using Vlingo.Xoom.Lattice.Grid.Cache;
using Vlingo.Xoom.Lattice.Grid.Hashring;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid
{
    public class Grid : Stage, IGridRuntime, IDisposable
    {
        private const int GridStageBuckets = 32;
        private const int GridStageInitialCapacity = 16_384;

        private readonly ILogger _logger;

        private static readonly string InstanceName = Guid.NewGuid().ToString();

        private readonly IHashRing<Id> _hashRing;
        
        private List<Node> _liveNodes = new List<Node>();

        private volatile bool _hasQuorum;
        private readonly long _clusterHealthCheckInterval;
        
        private readonly string _clusterAppStageName;

        private Thread? _runnableThread;
        
        public static Grid Instance(World world) => world.ResolveDynamic<Grid>(InstanceName);
        
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
            _hashRing = new MurmurSortedMapHashRing<Id>(100, (i, id) => new CacheNodePoint<Id>(Cache.Cache.Of(name), i, id));
            _clusterAppStageName = clusterProperties.ClusterApplicationStageName();
            ExtenderStartDirectoryScanner();
            GridNodeBootstrap = GridNodeBootstrap.Boot(this, name, clusterProperties, false);
            _hasQuorum = false;
            WorldClassLoader = new DynaClassLoader();

            _clusterHealthCheckInterval = clusterProperties.ClusterHealthCheckInterval();

            world.RegisterDynamic(InstanceName, this);
        }

        protected override Func<IAddress?, IMailbox?, IMailbox?> MailboxWrapper() => 
            (a, m) => new GridMailbox(m!, NodeId!, a!, _hashRing, Outbound!, _logger);
        
        public void Terminate() => World.Terminate();
        
        public void QuorumAchieved() => _hasQuorum = true;

        public void QuorumLost() => _hasQuorum = false;

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
            var copy = _hashRing.Copy();
            _hashRing.ExcludeNode(NodeId!);

            AllActorAddresses(this)
                .Where(address => address.IsDistributable && IsAssignedTo(copy, address, NodeId!))
                .ToList()
                .ForEach(address => {
                    var actor = ActorOf(this, address);
                    var toNode = _hashRing.NodeOf(address.IdString);
                    if (toNode != null)
                    {
                        // last node in the cluster?
                        RelocateActorTo(actor!, address, toNode);
                    }
                });
        }

        public Stage AsStage() => this;

        public void NodeJoined(Id newNode)
        {
            if (NodeId!.Equals(newNode))
            {
                // self is added to the hash-ring on GridNode#start
                return;
            }

            var copy = _hashRing.Copy();
            _hashRing.IncludeNode(newNode);

            AllActorAddresses(this)
                .Where(address =>
                    address.IsDistributable && ShouldRelocateTo(copy, address, newNode)).ToList()
                .ForEach(address => {
                    var actor = ActorOf(this, address);
                    RelocateActorTo(actor!, address, newNode);
                });
        }
        
        public Id? NodeId { get; set; }
        public IOutbound? Outbound { get; set; }

        public GridNodeBootstrap GridNodeBootstrap { get; }

        public IHashRing<Id> HashRing => _hashRing;
        public IQuorumObserver QuorumObserver => this;
        public DynaClassLoader WorldClassLoader { get; }
        
        protected override T ActorThunkFor<T>(Definition definition, IAddress? address)
        {
            var actorMailbox = AllocateMailbox(definition, address, null);
            actorMailbox.SuspendExceptFor(GridActorOperations.Resume, typeof(IRelocatable));
            var actor =
                ActorProtocolFor<T>(
                    definition,
                    definition.ParentOr(World.DefaultParent),
                    address,
                    actorMailbox,
                    definition.Supervisor,
                    definition.LoggerOr(World.DefaultLogger));

            return actor!.ProtocolActor;
        }

        public void InformAllLiveNodes(IEnumerable<Node> liveNodes) => _liveNodes = liveNodes.ToList();

        private void RelocateActorTo(Actor actor, IAddress address, Id toNode)
        {
            if (!GridActorOperations.IsSuspendedForRelocation(actor))
            {
                _logger.Debug($"Relocating actor [{address}] to [{toNode}]");
                //actor.suspendForRelocation();
                GridActorOperations.SuspendForRelocation(actor);
                Outbound?.Relocate(
                    toNode,
                    NodeId!,
                    Definition.SerializationProxy.From(actor.Definition),
                    address,
                    GridActorOperations.SupplyRelocationSnapshot(actor) /*actor.provideRelocationSnapshot()*/,
                    GridActorOperations.Pending(actor));
            }
        }
        
        private bool ShouldRelocateTo(IHashRing<Id> previous, IAddress address, Id newNode) =>
            IsAssignedTo(previous, address, NodeId!)
            && IsAssignedTo(_hashRing, address, newNode);

        private static bool IsAssignedTo(IHashRing<Id> ring, IAddress a, Id node) => 
            node.Equals(ring.NodeOf(a.IdString));
        
        public List<Id> AllOtherNodes() =>
            _liveNodes
                .Select(n => n.Id)
                .Where(nodeId => !nodeId.Equals(NodeId))
                .ToList();

        public Stage LocalStage() => World.StageNamed(_clusterAppStageName);

        //====================================
        // Internal implementation
        //====================================
        
        internal override ActorProtocolActor<T>? ActorProtocolFor<T>(
            Definition definition,
            Actor? parent,
            IAddress? maybeAddress,
            IMailbox? maybeMailbox,
            ISupervisor? maybeSupervisor,
            ILogger logger)
        {
            var address = maybeAddress == null ? AddressFactory.Unique() : maybeAddress;
            var node = _hashRing.NodeOf(address.IdString);
            var mailbox = MaybeRemoteMailbox(address, definition, maybeMailbox!, () => {
                Outbound?.Start(node, NodeId!, typeof(T), address, Definition.SerializationProxy.From(definition)); // TODO remote start all protocols
            });
            
            return base.ActorProtocolFor<T>(definition, parent, maybeAddress, mailbox, maybeSupervisor, logger);
        }

        internal override ActorProtocolActor<object>[]? ActorProtocolFor(
            Type[] protocols,
            Definition definition,
            Actor? parent,
            IAddress? maybeAddress,
            IMailbox? maybeMailbox,
            ISupervisor? maybeSupervisor,
            ILogger logger)
        {
            var address = maybeAddress == null ? AddressFactory.Unique() : maybeAddress;
            var node = _hashRing.NodeOf(address.IdString);
            var mailbox = MaybeRemoteMailbox(address, definition, maybeMailbox!, () => {
                Outbound?.Start(node, NodeId!, protocols[0], address, Definition.SerializationProxy.From(definition)); // TODO remote start all protocols
            });
            
            return base.ActorProtocolFor(protocols, definition, parent, maybeAddress, mailbox, maybeSupervisor, logger);
        }
        
        private IMailbox MaybeRemoteMailbox(IAddress address, Definition definition, IMailbox maybeMailbox, ThreadStart @out)
        {
            while (!_hasQuorum && address.IsDistributable)
            {
                _logger.Debug("Mailbox allocation waiting for cluster quorum...");
                try
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(_clusterHealthCheckInterval));
                }
                catch (ThreadInterruptedException e)
                {
                    throw new Exception("The thread was interrupted", e);
                }
            }

            var node = _hashRing.NodeOf(address.IdString);
            IMailbox mailbox;
            if (node != null && !node.Equals(NodeId))
            {
                _runnableThread = new Thread(@out);
                _runnableThread.Start();
                mailbox = AllocateMailbox(definition, address, maybeMailbox);
                if (!mailbox.IsSuspendedFor(GridActorOperations.Resume))
                {
                    mailbox.SuspendExceptFor(GridActorOperations.Resume, typeof(IRelocatable));
                }
            }
            else
            {
                mailbox = maybeMailbox;
            }
            
            return mailbox;
        }

        public void Dispose()
        {
            try
            {
                _runnableThread?.Abort();
            }
            catch
            {
                // nothing new
            }
        }
    }
}