// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Concurrent;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Lattice.Exchange;

namespace Vlingo.Xoom.Lattice.Tests.Exchange
{
    public class TestExchangeReceiver1 : DefaultExchangeReceiver<LocalType1>
    {
        private AccessSafely _access = AccessSafely.AfterCompleting(0);
        private readonly ConcurrentQueue<object> _results;
        private readonly ILogger _logger;

        public TestExchangeReceiver1(ConcurrentQueue<object> results, ILogger logger)
        {
            _results = results;
            _logger = logger;
        }

        public override void Receive(LocalType1 message)
        {
            _logger.Debug($"TestExchangeReceiver1 receiving: {message}");
            _access.WriteUsing("addMessage", message);
        }
        
        public AccessSafely AfterCompleting(int times)
        {
            _access = AccessSafely.AfterCompleting(times);
            _access
                .WritingWith<LocalType1>("addMessage", message => _results.Enqueue(message))
                .ReadingWith("getMessage", () =>
                    {
                        if (_results.TryDequeue(out var localType))
                        {
                            return (LocalType1) localType;
                        }

                        return null;
                    });

            return _access;
        }
    }
}