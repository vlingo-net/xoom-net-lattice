// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Lattice.Exchange;
using IMessage = Vlingo.Xoom.Common.Message.IMessage;

namespace Vlingo.Xoom.Lattice.Tests.Exchange;

public class TestExchange : IExchange, IMessageQueueListener
{
    private AccessSafely _access;
    private readonly IMessageQueue _queue;
    private readonly ILogger _logger;
    private readonly Forwarder _forwarder;
    private readonly AtomicInteger _sentCount = new AtomicInteger(0);
        
    public TestExchange(IMessageQueue queue, ILogger logger)
    {
        _queue = queue;
        _logger = logger;
        _queue.RegisterListener(this);
        _forwarder = new Forwarder();
        _access = AccessSafely.AfterCompleting(0);
    }

    public void Close() => _queue.Close(true);

    public T Channel<T>() => default;

    public T Connection<T>() => default;

    public string Name => "TestExchange";
        
    public IExchange Register<TLocal, TExternal, TExchange>(Covey<TLocal, TExternal, TExchange> covey)
    {
        _forwarder.Register(covey);
        return this;
    }

    public void Send<TLocal>(TLocal local)
    {
        _logger.Debug($"Exchange sending: {local}");
        _forwarder.ForwardToSender(local);
    }

    public void HandleMessage(IMessage message)
    {
        _logger.Debug($"Exchange receiving: {message}");
        _forwarder.ForwardToReceiver(message);
        _access.WriteUsing("sentCount", 1);
    }
        
    public AccessSafely AfterCompleting(int times)
    {
        _access = AccessSafely.AfterCompleting(times);
        _access
            .WritingWith<int>("sentCount", increment => _sentCount.AddAndGet(increment))
            .ReadingWith("sentCount", () => _sentCount.Get());

        return _access;
    }
}