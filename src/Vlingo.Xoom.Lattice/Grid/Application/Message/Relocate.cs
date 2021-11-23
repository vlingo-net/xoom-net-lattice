// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    [Serializable]
    public class Relocate : IMessage
    {
        public Type Protocol { get; }
        public IAddress Address { get; }
        public Definition.SerializationProxy Definition { get; }
        public object Snapshot { get; }
        public List<Deliver> Pending { get; }

        public Relocate(Type protocol, IAddress address, Definition.SerializationProxy definition, object snapshot, List<Deliver> pending)
        {
            Protocol = protocol;
            Address = address;
            Definition = definition;
            Snapshot = snapshot;
            Pending = pending;
        }
        
        public override string ToString() =>
            $"Relocate(address='{Address}', definitionProxy='{Definition}', snapshot='{Snapshot}', pending='{Pending}')";
    }
}