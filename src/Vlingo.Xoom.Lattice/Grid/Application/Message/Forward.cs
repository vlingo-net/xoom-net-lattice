// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    [Serializable]
    public class Forward : IMessage
    {
        public Id OriginalSender { get; }
        public IMessage Message { get; }

        public Forward(Id originalSender, IMessage message)
        {
            OriginalSender = originalSender;
            Message = message;
        }
        
        public void Accept(Id receiver, Id sender, IVisitor visitor) => visitor.Visit(receiver, sender, this);

        public override string ToString() => $"Forward(originalSender='{OriginalSender}', message='{Message}')";
    }
}