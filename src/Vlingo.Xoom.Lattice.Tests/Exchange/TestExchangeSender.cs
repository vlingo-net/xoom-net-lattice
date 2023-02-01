// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Lattice.Exchange;

namespace Vlingo.Xoom.Lattice.Tests.Exchange;

public class TestExchangeSender : DefaultExchangeSender<ExchangeMessage>
{
    private readonly ILogger _logger;

    public TestExchangeSender(IMessageQueue queue, ILogger logger)
    {
        _logger = logger;
        Queue = queue;
    }

    public override void Send(ExchangeMessage message)
    {
        _logger.Debug($"MessageQueue sending: {message}");
        Queue.Enqueue(message);
    }
        
    public IMessageQueue Queue { get; }
}