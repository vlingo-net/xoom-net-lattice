// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    [Serializable]
    public class Start : IMessage
    {
        public Type Protocol { get; }
        public IAddress Address { get; }
        public Definition.SerializationProxy Definition { get; }

        public Start(Type protocol, IAddress address, Definition.SerializationProxy definition)
        {
            Protocol = protocol;
            Address = address;
            Definition = definition;
        }
        
        public override string ToString() => $"Start(protocol='{Protocol.Name}', address='{Address}', definition='{Definition}')";
    }
}