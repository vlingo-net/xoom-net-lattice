// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Wire.Node;

namespace Vlingo.Lattice.Lattice.Grid.Application
{
    public interface IGridActorControl
    {
        // void Start<T>(Id recipient, Id sender, T protocol, IAddress address, Definition.SerializationProxy definitionProxy);
        //
        // void Deliver<T>(
        //         Id recipient,
        //         Id sender,
        //         ICompletes<T> returns,
        //         T protocol,
        //         IAddress address,
        //         Definition.SerializationProxy definitionProxy,
        //         SerializableConsumer<T> consumer,
        //         string representation);
        //
        // void Answer<T>(Id receiver, Id sender, IAnswer<T> answer);
        //
        // void Forward(Id receiver, Id sender, IMessage message);
        //
        // void Relocate(
        //     Id receiver,
        //     Id sender,
        //     Definition.SerializationProxy definitionProxy,
        //     IAddress address,
        //     object snapshot,
        //     IEnumerable<IMessage> pending);
    }

    public interface IInbound : IGridActorControl
    {
    }

    public interface IOutbound : IGridActorControl
    {
    }
}