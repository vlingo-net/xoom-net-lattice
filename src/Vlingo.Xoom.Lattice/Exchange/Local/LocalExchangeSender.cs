// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Message;

namespace Vlingo.Xoom.Lattice.Exchange.Local
{
    public class LocalExchangeSender : DefaultExchangeSender<LocalExchangeMessage>
    {
        public IMessageQueue Queue { get; }

        public LocalExchangeSender(IMessageQueue queue) => Queue = queue;

        public override void Send(LocalExchangeMessage message) => Queue.Enqueue(message);
    }
}