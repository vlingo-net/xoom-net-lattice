// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Streams.Operator;

namespace Vlingo.Xoom.Lattice.Exchange.Streams
{
    /// <summary>
    /// <see cref="IExchange"/> specific <see cref="QueueSource{T}"/> implementation.
    /// </summary>
    /// <typeparam name="T">Type of the message.</typeparam>
    public class ExchangeStreamSource<T> : QueueSource<T>
    {
        public ExchangeStreamSource(bool slow) : base(slow)
        {
        }
    }
}