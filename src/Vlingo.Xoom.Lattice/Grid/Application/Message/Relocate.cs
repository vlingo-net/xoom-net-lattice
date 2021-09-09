// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    [Serializable]
    public class Relocate<T> : IMessage
    {
        public IAddress Address { get; }
        public Definition.SerializationProxy<T> Definition { get; }
        public object Snapshot { get; }
        public List<Deliver<T>> Pending { get; }

        public Relocate(IAddress address, Definition.SerializationProxy<T> definition, object snapshot, List<Deliver<T>> pending)
        {
            Address = address;
            Definition = definition;
            Snapshot = snapshot;
            Pending = pending;
        }
        
        public void Accept(Id receiver, Id sender, IVisitor visitor) => visitor.Visit(receiver, sender, this);

        public override string ToString() =>
            $"Relocate(address='{Address}', definitionProxy='{Definition}', snapshot='{Snapshot}', pending='{Pending}')";
    }
}