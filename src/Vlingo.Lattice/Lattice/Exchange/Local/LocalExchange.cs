// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common.Message;

namespace Vlingo.Lattice.Exchange.Local
{
    public class LocalExchange : IExchange, IMessageQueueListener
    {
        private readonly object _channel = new object();
        private readonly object _connection = new object();
        private readonly IMessageQueue _queue;
        private readonly Forwarder _forwarder;
        
        public LocalExchange(IMessageQueue queue)
        {
            _queue = queue;
            queue.RegisterListener(this);
            _forwarder = new Forwarder();
        }
        
        public void Close() => _queue.Close(true);

        public T Channel<T>() => (T) _channel;

        public T Connection<T>() => (T) _connection;

        public string Name { get; } = "LocalExchange";
        
        public IExchange Register<TLocal, TExternal, TExchange>(Covey<TLocal, TExternal, TExchange> covey)
        {
            _forwarder.Register(covey);
            return this;
        }

        public void Send<TLocal>(TLocal local) => _forwarder.ForwardToSender(local);

        public void HandleMessage(IMessage message) => _forwarder.ForwardToReceiver(message);
    }
}