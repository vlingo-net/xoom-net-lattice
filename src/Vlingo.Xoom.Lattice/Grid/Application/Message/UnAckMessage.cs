// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    /// <summary>
    /// This class represents an unacknowledged message which has been sent to recipient.
    /// </summary>
    public class UnAckMessage : IMessage
    {
        public Type Protocol { get; }
        public Id Receiver { get; }
        public ICompletes Completes { get; }
        public GridDeliver Message { get; }

        public UnAckMessage(Type protocol, Id receiver, ICompletes completes, GridDeliver message)
        {
            Protocol = protocol;
            Receiver = receiver;
            Completes = completes;
            Message = message;
        }
    }
}