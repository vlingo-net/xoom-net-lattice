// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Lattice.Exchange
{
    public interface IExchangeReceiver
    {
        void Receive(object message);
    }
    
    /// <summary>
    /// A receiver of messages from an <see cref="IExchange"/>, which may be implemented for each unique message type.
    /// The <typeparamref name="T"/> has already been mapped and adapted from the exchange-typed message.
    /// </summary>
    /// <typeparam name="T">The type of the local message</typeparam>
    public interface IExchangeReceiver<in T> : IExchangeReceiver
    {
        /// <summary>
        /// Delivers the <typeparamref name="T"/> local typed message from the exchange to the receiver.
        /// The <typeparamref name="T"/> has already been mapped and adapted from the exchange-typed message.
        /// </summary>
        /// <param name="message">The <typeparamref name="T"/> typed local message.</param>
        void Receive(T message);
    }
}