// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    [Serializable]
    public class Start<T> : IMessage
    {
        public IAddress Address { get; }
        public Definition.SerializationProxy<T> Definition { get; }

        public Start(IAddress address, Definition.SerializationProxy<T> definition)
        {
            Address = address;
            Definition = definition;
        }
        
        public void Accept(Id receiver, Id sender, IVisitor visitor) => visitor.Visit(receiver, sender, this);

        public override string ToString() => $"Start(protocol='{typeof(T).Name}', address='{Address}', definition='{Definition}')";
    }
}