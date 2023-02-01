// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Grid.Application.Message;
using Vlingo.Xoom.Wire.Fdx.Outbound;
using Vlingo.Xoom.Wire.Nodes;
using IMessage = Vlingo.Xoom.Lattice.Grid.Application.Message.IMessage;

namespace Vlingo.Xoom.Lattice.Grid.Application;

public interface IGridActorControl
{
    void Start(Id recipient, Id sender, Type protocol, IAddress address, Definition.SerializationProxy definitionProxy);
        
    void GridDeliver(
        Id recipient,
        Id sender,
        ICompletes? returns,
        Type protocol,
        IAddress address,
        Definition.SerializationProxy definitionProxy,
        LambdaExpression consumer,
        string representation);
        
    void ActorDeliver(
        Id recipient,
        Id sender,
        ICompletes? returns,
        Type protocol,
        Func<Grid, Actor> actorProvider,
        LambdaExpression consumer,
        string representation);


    void Answer<T>(Id receiver, Id sender, Answer<T> answer);
        
    void Forward(Id receiver, Id sender, IMessage message);
        
    void Relocate(
        Id receiver,
        Id sender,
        Definition.SerializationProxy definitionProxy,
        IAddress address,
        object snapshot,
        IEnumerable<Vlingo.Xoom.Actors.IMessage> pending);
        
    void InformNodeIsHealthy(Id id, bool isHealthy);
}

public interface IInbound : IGridActorControl
{
}

public interface IOutbound : IGridActorControl
{
    void UseStream(IApplicationOutboundStream? outbound);
}