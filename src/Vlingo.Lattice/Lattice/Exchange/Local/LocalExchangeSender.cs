// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common.Message;

namespace Vlingo.Lattice.Exchange.Local
{
    public class LocalExchangeSender : IExchangeSender<LocalExchangeMessage>
    {
        public IMessageQueue Queue { get; }

        public LocalExchangeSender(IMessageQueue queue) => Queue = queue;

        public void Send(LocalExchangeMessage message) => Queue.Enqueue(message);
    }
}