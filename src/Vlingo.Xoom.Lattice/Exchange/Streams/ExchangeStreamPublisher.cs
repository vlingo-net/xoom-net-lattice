// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Streams;

namespace Vlingo.Xoom.Lattice.Exchange.Streams;

/// <summary>
/// This class receives messages from an <see cref="IExchange"/> and streams them using <see cref="StreamPublisher{T}"/> capabilities.
/// This class requires careful <see cref="PublisherConfiguration"/>, especially <code>Streams.OverflowPolicy</code> in conjunction with <see cref="ExchangeStreamSource{T}"/> source.
/// Low numeric values for <code>PublisherConfiguration.BufferSize</code> reduces memory usage (heap) while overflow drop rate may increase.
/// On the other hand, high numeric values for <code>PublisherConfiguration.BufferSize</code> decrease overflow drop rate while increasing memory usage (heap).
/// </summary>
/// <typeparam name="T">Type of the message.</typeparam>
public class ExchangeStreamPublisher<T> : StreamPublisher<T>, IExchangeReceiver<T>
{
    private ExchangeStreamSource<T> source;
        
    public ExchangeStreamPublisher(ISource<T> source, PublisherConfiguration configuration) : base(source, configuration) => 
        this.source = (ExchangeStreamSource<T>) source;

    public void Receive(T message) => source.Add(message);

    public void Receive(object message) => source.Add((T) message);
}