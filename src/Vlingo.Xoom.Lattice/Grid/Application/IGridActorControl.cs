// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
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

namespace Vlingo.Xoom.Lattice.Grid.Application
{
    public interface IGridActorControl
    {
        void Start<T>(Id recipient, Id sender, IAddress address, Definition.SerializationProxy<T> definitionProxy);
        
        void Deliver<T>(
                Id recipient,
                Id sender,
                ICompletes<T>? returns,
                IAddress address,
                Definition.SerializationProxy<T> definitionProxy,
                Expression<Action<T>> consumer,
                string representation);
        
        void Answer<T>(Id receiver, Id sender, Answer<T> answer);
        
        void Forward(Id receiver, Id sender, IMessage message);
        
        void Relocate<T>(
            Id receiver,
            Id sender,
            Definition.SerializationProxy<T> definitionProxy,
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
        void UseStream(IApplicationOutboundStream outbound);
    }
}