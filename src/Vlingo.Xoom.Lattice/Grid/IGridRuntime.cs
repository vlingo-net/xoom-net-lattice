// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common.Compiler;
using Vlingo.Xoom.Lattice.Grid.Application;
using Vlingo.Xoom.Lattice.Grid.Hashring;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid;

public interface IGridRuntime : IQuorumObserver
{
    Actor ActorAt(IAddress address);
    void RelocateActors();
    Stage AsStage();
    void NodeJoined(Id newNode);
    void InformAllLiveNodes(IEnumerable<Node> liveNodes);
    Id? NodeId { get; set; }
    IOutbound? Outbound { get; set; }
    World World { get; }
    GridNodeBootstrap GridNodeBootstrap { get; }
    IHashRing<Id> HashRing { get; }
    IQuorumObserver QuorumObserver { get; }
    DynaClassLoader WorldClassLoader { get; }
}