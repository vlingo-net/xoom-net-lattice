// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Lattice.Exchange
{
    /// <summary>
    /// A sender of messages to a <see cref="IExchange"/>.
    /// </summary>
    /// <typeparam name="T">The exchange typed message</typeparam>
    public interface IExchangeSender<in T>
    {
        /// <summary>
        ///  Sends the exchange typed message through the exchange.
        /// </summary>
        /// <param name="message">The <typeparamref name="T"/> typed exchange message to send</param>
        void Send(T message);
    }
}