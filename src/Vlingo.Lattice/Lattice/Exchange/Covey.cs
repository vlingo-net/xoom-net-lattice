// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Lattice.Exchange
{
    /// <summary>
    /// A set of <see cref="IExchange"/> components.
    /// </summary>
    /// <typeparam name="TLocal">The local object type</typeparam>
    /// <typeparam name="TExternal">The external object type</typeparam>
    /// <typeparam name="TExchange">The exchange message type</typeparam>
    public class Covey<TLocal, TExternal, TExchange>
    {
        public IExchangeAdapter<TLocal, TExternal, TExchange> Adapter { get; }
        public IExchangeSender<TExchange> Sender { get; }
        public IExchangeReceiver<TLocal> Receiver { get; }
        public Type LocalType => typeof(TLocal);
        public Type ExternalType => typeof(TExternal);
        
        /// <summary>
        /// Gets the <see cref="Covey{TLocal,TExternal,TExchange}"/> information from a set of related components, which includes
        /// <see cref="IExchangeSender{T}"/>, <see cref="IExchangeReceiver{T}"/>, <see cref="IExchangeAdapter{TLocal,TExternal,TExchange}"/>, and the classes of the local and
        /// channel types. These will be used to send and receive a specific kind of messages and to adapt such from/to
        /// channel and local types.
        /// </summary>
        /// <param name="sender">The <see cref="IExchangeSender{T}"/> through which a local message type is sent but first adapted to a channel message</param>
        /// <param name="receiver">The <see cref="IExchangeReceiver{T}"/> through which messages are received as local types</param>
        /// <param name="adapter">the <see cref="IExchangeAdapter{TLocal,TExternal,TExchange}"/> that adapts/maps local messages to channel and channel messages to local</param>
        /// <returns><see cref="Covey{TLocal,TExternal,TExchange}"/></returns>
        public static Covey<TLocal, TExternal, TExchange> Of(IExchangeSender<TExchange> sender, IExchangeReceiver<TLocal> receiver, IExchangeAdapter<TLocal, TExternal, TExchange> adapter)
            => new Covey<TLocal, TExternal, TExchange>(sender, receiver, adapter);

        /// <summary>
        /// Constructs the convey information from a set of related components, which includes the <see cref="Covey{TLocal,TExternal,TExchange}"/> information from a set of related components, which includes
        /// <see cref="IExchangeSender{T}"/>, <see cref="IExchangeReceiver{T}"/>, <see cref="IExchangeAdapter{TLocal,TExternal,TExchange}"/>, and the classes of the local and
        /// channel types. These will be used to send and receive a specific kind of messages and to adapt such from/to
        /// channel and local types.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        /// <param name="adapter"></param>
        public Covey(IExchangeSender<TExchange> sender, IExchangeReceiver<TLocal> receiver, IExchangeAdapter<TLocal, TExternal, TExchange> adapter)
        {
            Sender = sender;
            Receiver = receiver;
            Adapter = adapter;
        }
    }
}