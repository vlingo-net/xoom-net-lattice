// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    /// <summary>
    /// This class represents an unacknowledged message which has been sent to recipient.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    public class UnAckMessage<T>
    {
        public Id Receiver { get; }
        //public Returns<T> Returns { get; }
        public Deliver<T> Message { get; }

        public UnAckMessage(Id receiver/*, Returns<T> returns*/, Deliver<T> message)
        {
            Receiver = receiver;
            //Returns = returns;
            Message = message;
        }
    }
}