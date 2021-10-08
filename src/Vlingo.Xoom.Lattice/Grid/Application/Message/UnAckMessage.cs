// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    /// <summary>
    /// This class represents an unacknowledged message which has been sent to recipient.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    public class UnAckMessage<T> : IMessage
    {
        public Id Receiver { get; }
        public ICompletes<T> Completes { get; }
        public Deliver<T> Message { get; }

        public UnAckMessage(Id receiver, ICompletes<T> completes, Deliver<T> message)
        {
            Receiver = receiver;
            Completes = completes;
            Message = message;
        }

        public void Accept(Id receiver, Id sender, IVisitor visitor)
        {
        }
    }
}